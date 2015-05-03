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
  public class DependenciesPlugin : Plugin {
    public static readonly DependenciesPlugin Instance = new DependenciesPlugin();
    public const string NuGetRemoteRepoUri = "http://packages.nuget.org/api/v2";

    public override Settings Setup(Settings settings) {
      return settings.AddGlobally(DependenciesKeys.ExternalDependenciesKeys.Init(ImmutableHashSet<ConfigKey<ImmutableList<ExternalDependency>>>.Empty),
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
        DownloadMissingDependencies(config, fetchedDependencies);
        return fetchedDependencies;
      }
      if (config.GetExternalDependencies().IsEmpty) {
        return new FetchedDependencies(config, new JsonFetchedDependencies(ImmutableList<JsonPackageVersions>.Empty));
      }
      var fetchTask = FetchImpl(config);
      fetchTask.Wait();
      return fetchTask.Result;
    }

    private static void DownloadMissingDependencies(IConfig config, FetchedDependencies fetchedDependencies) {
      var missingDependencies = FetchedDependenciesUtil.MissingDependencies(fetchedDependencies);
      var packageManager = CreatePackageManager(config);
      InstallNuGetPackages(packageManager, missingDependencies);
    }

    private static Task<FetchedDependencies> FetchImpl(IConfig config) {
      return Task.Run(() => {
        var dependencies = config.GetExternalDependencies();
        var packageManager = CreatePackageManager(config);
        InstallNuGetPackages(packageManager, dependencies);
        var installedPackages = packageManager.LocalRepository.GetPackages().GroupBy(package => package.Id);
        return PersistFetchedDependencies(config, installedPackages);
      });
    }

    private static void CleanDependenciesImpl(IContext context) {
      var nuGetRepositoryDir = context.GetFetchedDependenciesDir();
      Directory.Delete(nuGetRepositoryDir, true);
    }

    private static bool TryLoadPersistedFetchedDependencies(IConfig config, out FetchedDependencies fetchedDependencies) {
      var fetchedPackagesFile = config.GetFetchedDependenciesListFile();
      if (File.Exists(fetchedPackagesFile)) {
        using (var streamReader = new StreamReader(fetchedPackagesFile)) {
          using (var jsonStreamReader = new JsonTextReader(streamReader)) {
            fetchedDependencies = new FetchedDependencies(config, JsonSerializer.CreateDefault().Deserialize<JsonFetchedDependencies>(jsonStreamReader));
            return true;
          }
        }
      }
      fetchedDependencies = null;
      return false;
    }

    private static PackageManager CreatePackageManager(IConfig context) {
      var nuGetRepositoryDir = context.GetFetchedDependenciesDir();
      IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository(NuGetRemoteRepoUri);
      return new PackageManager(repo, nuGetRepositoryDir);
    }

    private static void InstallNuGetPackages(IPackageManager packageManager, IEnumerable<ExternalDependency> dependencies) {
      foreach (var dependency in dependencies) {
        IPackage foundPackage;
        try {
          foundPackage = packageManager.SourceRepository.FindPackage(dependency.Id, dependency.Version, false, false);
          packageManager.InstallPackage(foundPackage, false, false);
        } catch (Exception) {
          foundPackage = packageManager.LocalRepository.FindPackage(dependency.Id, dependency.Version, false, false);
        }
        if (foundPackage == null) {
          throw new Exception(string.Format("Could not download dependency '{0}'. Please verify your build configuration.", dependency));
        }
      }
    }

    private static void InstallNuGetPackages(PackageManager packageManager, IEnumerable<Package> packagesToInstall) {
      foreach (var packageToInstall in packagesToInstall) {
        try {
          packageManager.InstallPackage(packageToInstall.Id, packageToInstall.Version);
        } catch (Exception e) {
          throw new Exception(string.Format("Could not download dependency '{0}'. Please check your internet connection of your build definition.", packageToInstall), e);
        }
      }
    }

    private static FetchedDependencies PersistFetchedDependencies(IConfig config, IEnumerable<IGrouping<string, IPackage>> fetchedPackages) {
      Directory.CreateDirectory(config.GetPersistentBuildConfigDir());
      var resolvedExternalDependencies = new JsonFetchedDependencies(fetchedPackages);
      using (var streamWriter = new StreamWriter(config.GetFetchedDependenciesListFile())) {
        using (var jsonTextWriter = new JsonTextWriter(streamWriter)) {
          jsonTextWriter.Formatting = Formatting.Indented;
          JsonSerializer.CreateDefault().Serialize(jsonTextWriter, resolvedExternalDependencies);
        }
      }
      return new FetchedDependencies(config, resolvedExternalDependencies);
    }

    private static string FetchedDependenciesFileImpl(IConfig context) {
      return Path.Combine(context.GetPersistentBuildConfigDir(), DependenciesSettings.FetchedPackagesFileName);
    }
  }
}