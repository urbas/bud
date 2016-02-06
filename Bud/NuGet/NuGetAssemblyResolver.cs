using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Bud.Util;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Repositories;
using NuGet.Versioning;
using static Bud.NuGet.FrameworkAssemblyReferencesAggregator;
using static Bud.NuGet.WindowsFrameworkAssemblyResolver;
using static Bud.Util.Option;

namespace Bud.NuGet {
  public class NuGetAssemblyResolver : IAssemblyResolver {
    public IEnumerable<string> FindAssembly(IEnumerable<PackageReference> packageReferences,
                                            string packagesDir,
                                            string cacheDir) {
      var packageRepository = CreatePackageRepository(packagesDir, cacheDir);
      var frameworkAssemblies = new List<FrameworkAssemblyReference>();
      var assemblies = new List<string>();
      foreach (var packageReference in packageReferences) {
        var packageInfo = FindBestMatch(packageRepository, packageReference);
        var nuspec = new NuspecReader(XDocument.Load(packageInfo.ManifestPath));

        frameworkAssemblies.AddRange(FindFrameworkAssemblyReferences(packageReference.Framework, nuspec.GetFrameworkReferenceGroups()));

        using (var fileStream = File.OpenRead(packageInfo.ZipPath)) {
          AddAssembliesFromPackage(packagesDir,
                                   fileStream,
                                   assemblies,
                                   frameworkAssemblies,
                                   nuspec.GetId(),
                                   nuspec.GetVersion(),
                                   packageReference.Framework);
        }
      }
      assemblies.AddRange(AggregateReferences(frameworkAssemblies)
                            .Gather(FindFrameworkAssembly));
      return assemblies;
    }

    private static void AddAssembliesFromPackage(string packagesDir,
                                                 Stream fileStream,
                                                 List<string> assemblies,
                                                 List<FrameworkAssemblyReference> frameworkAssemblies,
                                                 string packageId,
                                                 NuGetVersion packageVersion,
                                                 NuGetFramework targetFramework) {
      var nupkg = new PackageReader(fileStream, false);
      var packageAssemblies = FindAssemblies(nupkg, packagesDir, targetFramework, packageId, packageVersion);
      if (packageAssemblies.Count > 0) {
        assemblies.AddRange(packageAssemblies);
      } else {
        var reference = ToFrameworkAssemblyReference(nupkg, targetFramework);
        if (reference.HasValue) {
          frameworkAssemblies.Add(reference.Value);
        }
      }
    }

    private static NuGetv3LocalRepository CreatePackageRepository(string packagesDir, string cacheDir) {
      var packagesV3Dir = Path.Combine(cacheDir, "packages-index");
      NuGetExecutable.ExecuteNuGet($"init {packagesDir} {packagesV3Dir}");
      return new NuGetv3LocalRepository(packagesV3Dir, true);
    }

    private static Option<FrameworkAssemblyReference>
      ToFrameworkAssemblyReference(PackageReaderBase nupkg,
                                   NuGetFramework targetFramework) {
      var frameworkSpecificGroup = nupkg.GetReferenceItems().GetNearest(targetFramework);
      if (frameworkSpecificGroup == null) {
        return None<FrameworkAssemblyReference>();
      }
      return new FrameworkAssemblyReference(nupkg.GetIdentity().Id,
                                            frameworkSpecificGroup.TargetFramework.Version);
    }

    private static ICollection<string>
      FindAssemblies(PackageReaderBase nupkg,
                     string packagesDir,
                     NuGetFramework targetFramework,
                     string packageId,
                     NuGetVersion packageVersion) {
      var referenceItems = nupkg.GetReferenceItems()
                                .GetNearest(targetFramework)?
                                .Items ?? Enumerable.Empty<string>();
      return referenceItems.Select(pathInPackage => PathInFileSystem(packagesDir, pathInPackage, packageId, packageVersion))
                           .ToList();
    }

    private static string PathInFileSystem(string packagesDir,
                                           string pathInPackage,
                                           string packageId,
                                           NuGetVersion packageVersion)
      => Path.Combine(packagesDir, $"{packageId}.{packageVersion}", pathInPackage);

    private static IEnumerable<FrameworkAssemblyReference>
      FindFrameworkAssemblyReferences(NuGetFramework targetFramework,
                                      IEnumerable<FrameworkSpecificGroup> frameworkGroups) {
      var group = frameworkGroups.GetNearest(targetFramework);
      if (group == null) {
        return Enumerable.Empty<FrameworkAssemblyReference>();
      }
      var frameworkVersion = group.TargetFramework.Version;
      return group.Items
                  .Select(assemblyName => ToFrameworkAssemblyReference(assemblyName, frameworkVersion));
    }

    private static FrameworkAssemblyReference ToFrameworkAssemblyReference(string assemblyName, Version frameworkVersion) {
      return new FrameworkAssemblyReference(assemblyName,
                                            frameworkVersion);
    }

    private static LocalPackageInfo
      FindBestMatch(NuGetv3LocalRepository packageRepository,
                    PackageReference packageReference)
      => packageRepository
        .FindPackagesById(packageReference.Id)
        .FindBestMatch(new VersionRange(packageReference.Version), info => info?.Version);

    private static Option<string> FindFrameworkAssembly(KeyValuePair<string, Version> reference)
      => ResolveFrameworkAssembly(reference.Key, reference.Value);
  }
}