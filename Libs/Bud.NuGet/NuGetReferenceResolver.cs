using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Bud.Make;
using Bud.References;
using Newtonsoft.Json;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Repositories;
using NuGet.Versioning;

namespace Bud.NuGet {
  public class NuGetReferenceResolver : INuGetReferenceResolver {
    public ResolvedReferences Resolve(ImmutableArray<PackageReference> packageReferences,
                                      string outputDir) {
      var packageReferencesList = packageReferences;
      if (packageReferencesList.IsEmpty) {
        return ResolvedReferences.Empty;
      }
      var packagesConfigFile = PackageReference.WritePackagesConfigXml(packageReferencesList,
                                                                       Path.Combine(outputDir, "packages.config"));
      var nugetV2RepoDir = Path.Combine(outputDir, "packages");
      var nugetV3RepoDir = Path.Combine(outputDir, "pacakges-v3");
      HashBasedBuilder.Build(RestorePackages, ImmutableArray.Create(packagesConfigFile), nugetV2RepoDir);
      var assemblyPathsJsonFile = HashBasedBuilder.Build((inputfile, outputfile) => ResolveAssemblies(packageReferencesList, nugetV2RepoDir, nugetV3RepoDir, outputfile),
                                                         ImmutableArray.Create(packagesConfigFile),
                                                         Path.Combine(outputDir, "nuget_assemblies.json"));
      var assemblyPathsJson = File.ReadAllText(assemblyPathsJsonFile);
      return JsonConvert.DeserializeObject<ResolvedReferences>(assemblyPathsJson);
    }

    private static void RestorePackages(ImmutableArray<string> packagesConfigFiles, string packagesDir)
      => Exec.CheckCall("nuget", $"restore {packagesConfigFiles[0]} -PackagesDirectory {packagesDir}");

    private static void ResolveAssemblies(IEnumerable<PackageReference> packageReferences,
                                          string packagesDir,
                                          string indexDir,
                                          string assemblyPathsJsonFile) {
      var nugetRepo = CreateNuGetV3Repo(packagesDir, indexDir);
      var resolvedReferences = FindAssemblies(packageReferences, packagesDir, nugetRepo);
      var assemblyPathsJson = JsonConvert.SerializeObject(resolvedReferences, Formatting.Indented);
      File.WriteAllText(assemblyPathsJsonFile, assemblyPathsJson);
    }


    public static ResolvedReferences FindAssemblies(IEnumerable<PackageReference> packageReferences,
                                                    string packagesCacheDir,
                                                    NuGetv3LocalRepository localRepository) {
      var frameworkAssemblies = new List<FrameworkAssembly>();
      var assemblies = ImmutableArray.CreateBuilder<Assembly>();
      foreach (var packageReference in packageReferences) {
        var packageInfo = FindBestMatch(localRepository, packageReference);
        var nuspec = new NuspecReader(XDocument.Load(packageInfo.ManifestPath));

        frameworkAssemblies.AddRange(FindFrameworkAssemblies(packageReference.Framework, nuspec.GetFrameworkReferenceGroups()));

        using (var fileStream = File.OpenRead(packageInfo.ZipPath)) {
          AddAssembliesFromPackage(assemblies,
                                   packagesCacheDir,
                                   fileStream,
                                   frameworkAssemblies,
                                   nuspec.GetId(), nuspec.GetVersion(), packageReference.Framework);
        }
      }
      return new ResolvedReferences(assemblies.ToImmutable(),
                                    FrameworkAssembly.TakeHighestVersions(frameworkAssemblies).ToImmutableArray());
    }

    private static NuGetv3LocalRepository CreateNuGetV3Repo(string nugetV2RepoDir, string nugetV3RepoDir) {
      Exec.CheckCall("nuget", $"init {nugetV2RepoDir} {nugetV3RepoDir}");
      return new NuGetv3LocalRepository(nugetV3RepoDir, true);
    }

    private static void AddAssembliesFromPackage(ImmutableArray<Assembly>.Builder assemblies,
                                                 string packagesCacheDir,
                                                 Stream fileStream,
                                                 ICollection<FrameworkAssembly> frameworkAssemblies,
                                                 string packageId,
                                                 NuGetVersion packageVersion,
                                                 NuGetFramework targetFramework) {
      var nupkg = new PackageArchiveReader(fileStream, false);
      var frameworkSpecificGroup = nupkg.GetReferenceItems()
                                        .GetNearest(targetFramework);
      var referenceItems = frameworkSpecificGroup?.Items ?? Enumerable.Empty<string>();
      var referenceItemsList = referenceItems as IList<string> ?? referenceItems.ToList();
      if (referenceItemsList.Count > 0) {
        assemblies.AddRange(referenceItemsList.Select(pathInPackage => Path.Combine(packagesCacheDir, $"{packageId}.{packageVersion}", pathInPackage))
                                              .Select(Assembly.FromPath)
                                              .ToList());
      } else {
        if (frameworkSpecificGroup != null) {
          frameworkAssemblies.Add(new FrameworkAssembly(nupkg.GetIdentity().Id,
                                                        frameworkSpecificGroup.TargetFramework.Version));
        }
      }
    }

    private static IEnumerable<FrameworkAssembly> FindFrameworkAssemblies(NuGetFramework targetFramework,
                                                                          IEnumerable<FrameworkSpecificGroup> frameworkGroups) {
      var group = frameworkGroups.GetNearest(targetFramework);
      if (group == null) {
        return Enumerable.Empty<FrameworkAssembly>();
      }
      var frameworkVersion = group.TargetFramework.Version;
      return group.Items
                  .Select(assemblyName => new FrameworkAssembly(assemblyName, frameworkVersion));
    }

    private static LocalPackageInfo FindBestMatch(NuGetv3LocalRepository packageRepository,
                                                  PackageReference packageReference)
      => packageRepository
        .FindPackagesById(packageReference.Id)
        .FindBestMatch(new VersionRange(packageReference.Version), info => info?.Version);
  }
}