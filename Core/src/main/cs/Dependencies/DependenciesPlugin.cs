using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Build;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Dependencies {
  public class DependenciesPlugin {
    public static Settings Init(Settings settings) {
      return settings.Globally(DependenciesKeys.ExternalDependenciesKeys.Init(ImmutableHashSet<ConfigKey<ImmutableList<ExternalDependency>>>.Empty),
                               DependenciesKeys.DependenciesRepositoryDir.Init(context => Path.Combine(context.GetBudDir(), "nuGetRepository")),
                               DependenciesKeys.FetchedDependenciesFile.Init(PersistedPackagesListFileImpl),
                               DependenciesKeys.FetchDependencies.Init(FetchImpl),
                               DependenciesKeys.DependenciesClean.InitSync(CleanDependenciesImpl),
                               DependenciesKeys.FetchedDependencies.Init(NuGetResolvedPackagesImpl));
    }

    private static FetchedDependencies NuGetResolvedPackagesImpl(IConfig context) {
      FetchedDependencies resolution;
      if (TryLoadPersistedResolution(context, out resolution)) {
        return resolution;
      }
      return new FetchedDependencies(ImmutableList<PackageVersions>.Empty);
    }

    private static Task<FetchedDependencies> FetchImpl(IContext context) {
      return Task.Run(() => {
        var dependencies = context.GetExternalDependencies();
        var packageManager = CreatePackageManager(context);
        InstallNuGetPackages(packageManager, dependencies);
        var installedPackages = packageManager.LocalRepository.GetPackages().GroupBy(package => package.Id);
        return PersistNuGetResolution(context, installedPackages);
      });
    }

    private static void CleanDependenciesImpl(IContext context) {
      var nuGetRepositoryDir = context.GetNuGetRepositoryDir();
      Directory.Delete(nuGetRepositoryDir, true);
    }

    private static bool TryLoadPersistedResolution(IConfig context, out FetchedDependencies persistedResolution) {
      var fetchedPackagesFile = context.GetPersistedPackagesListFile();
      if (File.Exists(fetchedPackagesFile)) {
        using (var streamReader = new StreamReader(fetchedPackagesFile)) {
          using (var jsonStreamReader = new JsonTextReader(streamReader)) {
            persistedResolution = JsonSerializer.CreateDefault().Deserialize<FetchedDependencies>(jsonStreamReader);
            return true;
          }
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
        IPackage foundPackage;
        try {
          foundPackage = packageManager.SourceRepository.FindPackage(dependency.Id, dependency.Version, allowPrereleaseVersions: false, allowUnlisted: false);
          packageManager.InstallPackage(foundPackage, ignoreDependencies: false, allowPrereleaseVersions: false);
        } catch (Exception) {
          foundPackage = packageManager.LocalRepository.FindPackage(dependency.Id, dependency.Version, allowPrereleaseVersions: false, allowUnlisted: false);
        }
        if (foundPackage == null) {
          throw new Exception(String.Format("Could not download dependency '{0}'. Please verify your build configuration.", dependency));
        }
      }
    }

    private static FetchedDependencies PersistNuGetResolution(IContext context, IEnumerable<IGrouping<string, IPackage>> fetchedPackages) {
      context.CreatePersistentBuildConfigDir();
      var resolvedExternalDependencies = new FetchedDependencies(fetchedPackages);
      using (var streamWriter = new StreamWriter(context.GetPersistedPackagesListFile())) {
        using (var jsonTextWriter = new JsonTextWriter(streamWriter)) {
          jsonTextWriter.Formatting = Formatting.Indented;
          JsonSerializer.CreateDefault().Serialize(jsonTextWriter, resolvedExternalDependencies);
        }
      }
      return resolvedExternalDependencies;
    }

    private static string PersistedPackagesListFileImpl(IConfig context) {
      return Path.Combine(context.GetPersistentBuildConfigDir(), DependenciesSettings.FetchedPackagesFileName);
    }
  }
}