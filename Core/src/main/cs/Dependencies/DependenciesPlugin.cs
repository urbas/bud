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
    public const string NuGetRemoteRepoUri = "http://packages.nuget.org/api/v2";

    public static Settings Init(Settings settings) {
      return settings.Globally(DependenciesKeys.ExternalDependenciesKeys.Init(ImmutableHashSet<ConfigKey<ImmutableList<ExternalDependency>>>.Empty),
                               DependenciesKeys.FetchedDependenciesDir.Init(FetchedDependenciesDirImpl),
                               DependenciesKeys.FetchedDependenciesListFile.Init(FetchedDependenciesFileImpl),
                               DependenciesKeys.Fetch.Init(FetchImpl),
                               DependenciesKeys.CleanDependencies.InitSync(CleanDependenciesImpl),
                               DependenciesKeys.FetchedDependencies.Init(FetchedDependenciesImpl));
    }

    private static string FetchedDependenciesDirImpl(IConfig context) {
      return Path.Combine(context.GetBudDir(), "nuGetRepository");
    }

    private static FetchedDependencies FetchedDependenciesImpl(IConfig config) {
      FetchedDependencies fetchedDependencies;
      if (TryLoadPersistedFetchedDependencies(config, out fetchedDependencies)) {
        return fetchedDependencies.WithConfig(config);
      }
      if (config.GetExternalDependencies().IsEmpty) {
        return new FetchedDependencies(ImmutableList<PackageVersions>.Empty);
      }
      // TODO: throw an exception if any of the files listed in fetched dependencies is missing.
      throw new InvalidOperationException(string.Format("Could not load the list of fetched dependencies. Please run the '{0}' task first.", DependenciesKeys.Fetch));
    }

    private static Task<FetchedDependencies> FetchImpl(IContext context) {
      return Task.Run(() => {
        var dependencies = context.GetExternalDependencies();
        var packageManager = CreatePackageManager(context);
        InstallNuGetPackages(packageManager, dependencies);
        var installedPackages = packageManager.LocalRepository.GetPackages().GroupBy(package => package.Id);
        return PersistFetchedDependencies(context, installedPackages);
      });
    }

    private static void CleanDependenciesImpl(IContext context) {
      var nuGetRepositoryDir = context.GetFetchedDependenciesDir();
      Directory.Delete(nuGetRepositoryDir, true);
    }

    private static bool TryLoadPersistedFetchedDependencies(IConfig context, out FetchedDependencies persistedResolution) {
      var fetchedPackagesFile = context.GetFetchedDependenciesListFile();
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
      var nuGetRepositoryDir = context.GetFetchedDependenciesDir();
      IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository(NuGetRemoteRepoUri);
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

    private static FetchedDependencies PersistFetchedDependencies(IContext context, IEnumerable<IGrouping<string, IPackage>> fetchedPackages) {
      Directory.CreateDirectory(context.GetPersistentBuildConfigDir());
      var resolvedExternalDependencies = new FetchedDependencies(fetchedPackages);
      using (var streamWriter = new StreamWriter(context.GetFetchedDependenciesListFile())) {
        using (var jsonTextWriter = new JsonTextWriter(streamWriter)) {
          jsonTextWriter.Formatting = Formatting.Indented;
          JsonSerializer.CreateDefault().Serialize(jsonTextWriter, resolvedExternalDependencies);
        }
      }
      return resolvedExternalDependencies;
    }

    private static string FetchedDependenciesFileImpl(IConfig context) {
      return Path.Combine(context.GetPersistentBuildConfigDir(), DependenciesSettings.FetchedPackagesFileName);
    }
  }
}