using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Plugins.Build;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Plugins.Deps {
  public class DependenciesPlugin : IPlugin {

    public static readonly DependenciesPlugin Instance = new DependenciesPlugin();

    private DependenciesPlugin() {}

    public Settings ApplyTo(Settings settings, Key project) {
      return settings
        .Init(DependenciesKeys.ExternalDependenciesKeys, ImmutableList<ConfigKey<ImmutableList<ExternalDependency>>>.Empty)
        .Init(DependenciesKeys.NuGetRepositoryDir, context => Path.Combine(context.GetBudDir(), "nuGetRepository"))
        .Init(DependenciesKeys.Fetch, FetchImpl)
        .Init(DependenciesKeys.NuGetResolvedPackages, NuGetResolvedPackagesImpl);
    }

    private static NuGetPackages NuGetResolvedPackagesImpl(IConfig context) {
      NuGetPackages resolution;
      if (TryLoadPersistedResolution(context, out resolution)) {
        return resolution;
      }
      return new NuGetPackages(ImmutableList<PackageVersions>.Empty);
    }

    private static Task<NuGetPackages> FetchImpl(IContext context) {
      return Task.Run(() => {
        var dependencies = context.GetExternalDependencies();
        var packageManager = CreatePackageManager(context);
        InstallNuGetPackages(packageManager, dependencies);
        var installedPackages = packageManager.LocalRepository.GetPackages().GroupBy(package => package.Id);
        return PersistNuGetResolution(context, installedPackages);
      });
    }

    private static bool TryLoadPersistedResolution(IConfig context, out NuGetPackages persistedResolution) {
      var fetchedPackagesFile = context.GetFetchedPackagesFile();
      if (File.Exists(fetchedPackagesFile)) {
        using (var streamReader = new StreamReader(fetchedPackagesFile))
        using (var jsonStreamReader = new JsonTextReader(streamReader)) {
          persistedResolution = JsonSerializer.CreateDefault().Deserialize<NuGetPackages>(jsonStreamReader);
          return true;
        }
      }
      persistedResolution = null;
      return false;
    }

    private static PackageManager CreatePackageManager(IContext context) {
      var nuGetRepositoryDir = context.GetNuGetRepositoryDir();
      IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("http://packages.nuget.org/api/v2");
      return new PackageManager(repo, nuGetRepositoryDir);
    }

    private static void InstallNuGetPackages(IPackageManager packageManager, IEnumerable<ExternalDependency> dependencies) {
      foreach (var dependency in dependencies) {
        packageManager.InstallPackage(dependency.Id, dependency.Version, false, false);
      }
    }

    private static NuGetPackages PersistNuGetResolution(IContext context, IEnumerable<IGrouping<string, IPackage>> fetchedPackages) {
      context.CreatePersistentBuildConfigDir();
      var resolvedExternalDependencies = new NuGetPackages(fetchedPackages);
      using (var streamWriter = new StreamWriter(context.GetFetchedPackagesFile()))
      using (var jsonTextWriter = new JsonTextWriter(streamWriter)) {
        jsonTextWriter.Formatting = Formatting.Indented;
        JsonSerializer.CreateDefault().Serialize(jsonTextWriter, resolvedExternalDependencies);
      }
      return resolvedExternalDependencies;
    }
  }
}

