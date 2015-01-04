using System.Collections.Immutable;
using System.Threading.Tasks;
using NuGet;
using Bud.Plugins.Build;
using System.IO;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bud.Plugins.NuGet {
  public class NuGetPlugin : IPlugin {

    public static readonly NuGetPlugin Instance = new NuGetPlugin();

    private NuGetPlugin() {
    }

    public Settings ApplyTo(Settings settings, Key key) {
      return settings
        .Init(NuGetKeys.KeysWithNuGetDependencies, ImmutableList<Key>.Empty)
        .Init(NuGetKeys.NuGetRepositoryDir, context => Path.Combine(context.GetBudDir(), "nuGetRepository"))
        .Init(NuGetKeys.NuGetDependencies.In(key), ImmutableList<NuGetDependency>.Empty)
        .Init(NuGetKeys.Fetch, FetchImpl)
        .Modify(NuGetKeys.KeysWithNuGetDependencies, (context, oldValue) => oldValue.Add(key));
    }

    public static Task<NuGetResolution> FetchImpl(IEvaluationContext context) {
      return Task.Run(() => {
        NuGetResolution resolution;
        if (!TryLoadFetchSnapshot(context, out resolution)) {
          var fetchedPackages = InstallNuGetPackages(context);
          resolution = PersistNuGetResolution(context, fetchedPackages);
        }
        return resolution;
      });
    }

    private static bool TryLoadFetchSnapshot(IEvaluationContext context, out NuGetResolution fetchSnapshot) {
      var fetchedPackagesFile = context.GetFetchedPackagesFile();
      if (File.Exists(fetchedPackagesFile)) {
        using (var streamReader = new StreamReader(fetchedPackagesFile))
        using (var jsonStreamReader = new JsonTextReader(streamReader)) {
          fetchSnapshot = JsonSerializer.CreateDefault().Deserialize<NuGetResolution>(jsonStreamReader);
          return true;
        }
      }
      fetchSnapshot = null;
      return false;
    }

    static PackageManager CreatePackageManager(IEvaluationContext context) {
      var nuGetRepositoryDir = context.GetNuGetRepositoryDir();
      IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("http://packages.nuget.org/api/v2");
      PackageManager packageManager = new PackageManager(repo, nuGetRepositoryDir);
      return packageManager;
    }

    private static IEnumerable<IGrouping<string, IPackage>> InstallNuGetPackages(IEvaluationContext context) {
      var dependencies = context.GetNuGetDependencies();
      var packageManager = CreatePackageManager(context);
      foreach (var dependent2Dependencies in dependencies) {
        foreach (var dependency in dependent2Dependencies.Value) {
          packageManager.InstallPackage(dependency.PackageName, dependency.PackageVersion);
        }
      }
      return packageManager.LocalRepository.GetPackages().GroupBy(package => package.Id);
    }

    private static NuGetResolution PersistNuGetResolution(IEvaluationContext context, IEnumerable<IGrouping<string, IPackage>> fetchedPackages) {
      context.CreatePersistentBuildConfigDir();
      var nuGetResolution = new NuGetResolution(fetchedPackages);
      using (var streamWriter = new StreamWriter(context.GetFetchedPackagesFile()))
      using (var jsonTextWriter = new JsonTextWriter(streamWriter)) {
        jsonTextWriter.Formatting = Formatting.Indented;
        JsonSerializer.CreateDefault().Serialize(jsonTextWriter, nuGetResolution);
      }
      return nuGetResolution;
    }
  }
}

