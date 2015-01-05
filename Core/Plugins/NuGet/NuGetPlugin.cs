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
        .Init(NuGetKeys.NuGetResolvedPackages, NuGetResolvedPackagesImpl)
        .Modify(NuGetKeys.KeysWithNuGetDependencies, (context, oldValue) => oldValue.Add(key));
    }

    public static NuGetResolution NuGetResolvedPackagesImpl(IConfiguration context) {
      NuGetResolution resolution;
      if (TryLoadPersistedResolution(context, out resolution)) {
        return resolution;
      } else {
        return new NuGetResolution(ImmutableDictionary<string, Dictionary<SemanticVersion, IEnumerable<string>>>.Empty);
      }
    }

    public static Task<NuGetResolution> FetchImpl(IEvaluationContext context) {
      return Task.Run(() => {
        var dependencies = context.GetNuGetDependencies();
        var packageManager = CreatePackageManager(context);
        InstallNuGetPackages(packageManager, dependencies.Values.SelectMany(value => value));
        var installedPackages = packageManager.LocalRepository.GetPackages().GroupBy(package => package.Id);
        return PersistNuGetResolution(context, installedPackages);
      });
    }

    private static bool TryLoadPersistedResolution(IConfiguration context, out NuGetResolution persistedResolution) {
      var fetchedPackagesFile = context.GetFetchedPackagesFile();
      if (File.Exists(fetchedPackagesFile)) {
        using (var streamReader = new StreamReader(fetchedPackagesFile))
        using (var jsonStreamReader = new JsonTextReader(streamReader)) {
          persistedResolution = JsonSerializer.CreateDefault().Deserialize<NuGetResolution>(jsonStreamReader);
          return true;
        }
      }
      persistedResolution = null;
      return false;
    }

    static PackageManager CreatePackageManager(IEvaluationContext context) {
      var nuGetRepositoryDir = context.GetNuGetRepositoryDir();
      IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("http://packages.nuget.org/api/v2");
      return new PackageManager(repo, nuGetRepositoryDir);
    }

    private static void InstallNuGetPackages(IPackageManager packageManager, IEnumerable<NuGetDependency> dependencies) {
      foreach (var dependency in dependencies) {
        packageManager.InstallPackage(dependency.PackageName, dependency.PackageVersion, false, false);
      }
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

