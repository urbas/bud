using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Bud.References;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Repositories;
using NuGet.Versioning;
using static Bud.References.AssemblyAggregator;
using static Bud.References.WindowsFrameworkReferenceResolver;
using static Bud.Option;

namespace Bud.NuGet {
  public class NuGetAssemblyResolver : IAssemblyResolver {
    public IEnumerable<string> FindAssemblies(IEnumerable<PackageReference> packageReferences,
                                              string packagesCacheDir,
                                              string scratchDir) {
      var packageRepository = CreatePackageIndex(packagesCacheDir, scratchDir);
      var frameworkAssemblies = new List<FrameworkAssembly>();
      var assemblies = new List<string>();
      foreach (var packageReference in packageReferences) {
        var packageInfo = FindBestMatch(packageRepository, packageReference);
        var nuspec = new NuspecReader(XDocument.Load(packageInfo.ManifestPath));

        frameworkAssemblies.AddRange(FindFrameworkAssemblyReferences(packageReference.Framework, nuspec.GetFrameworkReferenceGroups()));

        using (var fileStream = File.OpenRead(packageInfo.ZipPath)) {
          AddAssembliesFromPackage(packagesCacheDir,
                                   fileStream,
                                   assemblies,
                                   frameworkAssemblies,
                                   nuspec.GetId(),
                                   nuspec.GetVersion(),
                                   packageReference.Framework);
        }
      }
      assemblies.AddRange(AggregateByFrameworkVersion(frameworkAssemblies)
                            .Gather(FindFrameworkAssembly));
      return assemblies;
    }

    private static void AddAssembliesFromPackage(string packagesCacheDir,
                                                 Stream fileStream,
                                                 List<string> assemblies,
                                                 List<FrameworkAssembly> frameworkAssemblies,
                                                 string packageId,
                                                 NuGetVersion packageVersion,
                                                 NuGetFramework targetFramework) {
      var nupkg = new PackageArchiveReader(fileStream, false);
      var packageAssemblies = FindAssemblies(nupkg, packagesCacheDir, targetFramework, packageId, packageVersion);
      if (packageAssemblies.Count > 0) {
        assemblies.AddRange(packageAssemblies);
      } else {
        var reference = ToFrameworkAssemblyReference(nupkg, targetFramework);
        if (reference.HasValue) {
          frameworkAssemblies.Add(reference.Value);
        }
      }
    }

    private static NuGetv3LocalRepository CreatePackageIndex(string packagesCacheDir, string scratchDir) {
      var packagesV3Dir = Path.Combine(scratchDir, "index");
      Exec.Run("nuget", $"init {packagesCacheDir} {packagesV3Dir}");
      return new NuGetv3LocalRepository(packagesV3Dir, true);
    }

    private static Option<FrameworkAssembly>
      ToFrameworkAssemblyReference(PackageReaderBase nupkg,
                                   NuGetFramework targetFramework) {
      var frameworkSpecificGroup = nupkg.GetReferenceItems().GetNearest(targetFramework);
      if (frameworkSpecificGroup == null) {
        return None<FrameworkAssembly>();
      }
      return new FrameworkAssembly(nupkg.GetIdentity().Id,
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

    private static IEnumerable<FrameworkAssembly>
      FindFrameworkAssemblyReferences(NuGetFramework targetFramework,
                                      IEnumerable<FrameworkSpecificGroup> frameworkGroups) {
      var group = frameworkGroups.GetNearest(targetFramework);
      if (group == null) {
        return Enumerable.Empty<FrameworkAssembly>();
      }
      var frameworkVersion = group.TargetFramework.Version;
      return group.Items
                  .Select(assemblyName => ToFrameworkAssemblyReference(assemblyName, frameworkVersion));
    }

    private static FrameworkAssembly ToFrameworkAssemblyReference(string assemblyName, Version frameworkVersion) {
      return new FrameworkAssembly(assemblyName,
                                   frameworkVersion);
    }

    private static LocalPackageInfo
      FindBestMatch(NuGetv3LocalRepository packageRepository,
                    PackageReference packageReference)
      => packageRepository
        .FindPackagesById(packageReference.Id)
        .FindBestMatch(new VersionRange(packageReference.Version), info => info?.Version);

    private static Option<string> FindFrameworkAssembly(FrameworkAssembly reference)
      => ResolveFrameworkAssembly(reference.Name, reference.FrameworkVersion);
  }
}