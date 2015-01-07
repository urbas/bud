using System;
using System.Linq;
using System.Collections.Immutable;
using System.IO;
using Bud.Plugins.Build;
using System.Collections.Generic;
using NuGet;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bud.Plugins.Dependencies {
  public class DependenciesPlugin : IPlugin {

    public static readonly DependenciesPlugin Instance = new DependenciesPlugin();

    private DependenciesPlugin() {}

    public Settings ApplyTo(Settings settings, Key key) {
      return settings
        .Init(DependenciesKeys.ExternalDependenciesKeys, ImmutableList<ConfigKey<ImmutableList<ExternalDependency>>>.Empty)
        .Init(DependenciesKeys.NuGetRepositoryDir, context => Path.Combine(context.GetBudDir(), "nuGetRepository"))
        .Init(DependenciesKeys.Fetch, FetchImpl)
        .Init(DependenciesKeys.NuGetResolvedPackages, NuGetResolvedPackagesImpl);
    }

    public static NuGetResolution NuGetResolvedPackagesImpl(IConfig context) {
      NuGetResolution resolution;
      if (TryLoadPersistedResolution(context, out resolution)) {
        return resolution;
      } else {
        return new NuGetResolution(ImmutableDictionary<string, Dictionary<SemanticVersion, IEnumerable<string>>>.Empty);
      }
    }

    public static Task<NuGetResolution> FetchImpl(IContext context) {
      return Task.Run(() => {
        var dependencies = context.GetExternalDependencies();
        var packageManager = CreatePackageManager(context);
        InstallNuGetPackages(packageManager, dependencies);
        var installedPackages = packageManager.LocalRepository.GetPackages().GroupBy(package => package.Id);
        return PersistNuGetResolution(context, installedPackages);
      });
    }

    private static bool TryLoadPersistedResolution(IConfig context, out NuGetResolution persistedResolution) {
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

    static PackageManager CreatePackageManager(IContext context) {
      var nuGetRepositoryDir = context.GetNuGetRepositoryDir();
      IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("http://packages.nuget.org/api/v2");
      return new PackageManager(repo, nuGetRepositoryDir);
    }

    private static void InstallNuGetPackages(IPackageManager packageManager, IEnumerable<ExternalDependency> dependencies) {
      foreach (var dependency in dependencies) {
        packageManager.InstallPackage(dependency.Id, dependency.Version, false, false);
      }
    }

    private static NuGetResolution PersistNuGetResolution(IContext context, IEnumerable<IGrouping<string, IPackage>> fetchedPackages) {
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

