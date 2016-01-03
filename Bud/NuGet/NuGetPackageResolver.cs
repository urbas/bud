using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Bud.Util;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Repositories;
using NuGet.Versioning;
using static Bud.NuGet.FrameworkAssemblyReferencesAggregator;
using static Bud.NuGet.WindowsFrameworkAssemblyResolver;
using static Bud.Util.Optional;

namespace Bud.NuGet {
  public class NuGetPackageResolver : IPackageResolver {
    public IEnumerable<string>
      Resolve(IReadOnlyCollection<string> packagesConfigFiles,
              string cacheDir) {
      var packagesDir = Path.Combine(cacheDir, "packages");
      foreach (var packagesConfigFile in packagesConfigFiles) {
        ExecuteNuGet($"restore {packagesConfigFile} -PackagesDirectory {packagesDir}");
      }
      var packagesV3Dir = Path.Combine(cacheDir, "packages-index");
      ExecuteNuGet($"init {packagesDir} {packagesV3Dir}");
      var packageReferences = packagesConfigFiles.SelectMany(PackageReferencesFromFile);
      var packageRepository = new NuGetv3LocalRepository(packagesV3Dir, true);
      var frameworkAssemblies = new List<Tuple<string, Version>>();
      var assemblies = new List<string>();
      foreach (var packageReference in packageReferences) {
        var packageInfo = FindBestMatch(packageRepository, packageReference);
        var nuspec = new NuspecReader(XDocument.Load(packageInfo.ManifestPath));

        frameworkAssemblies.AddRange(FindFrameworkAssemblyReferences(nuspec, packageReference));

        using (var fileStream = File.OpenRead(packageInfo.ZipPath)) {
          var nupkg = new PackageReader(fileStream, false);
          var packageAssemblies = FindAssemblies(nupkg, packageReference, packagesDir, nuspec);
          if (packageAssemblies.Count > 0) {
            assemblies.AddRange(packageAssemblies);
          } else {
            var reference = ToFrameworkAssemblyReference(nupkg, packageReference.TargetFramework);
            if (reference.HasValue) {
              frameworkAssemblies.Add(reference.Value);
            }
          }
        }
      }
      assemblies.AddRange(AggregateReferences(frameworkAssemblies)
        .Gather(FindFrameworkAssembly))
      ;
      return assemblies;
    }

    private static Optional<Tuple<string, Version>> ToFrameworkAssemblyReference(PackageReaderBase nupkg,
                                                                                 NuGetFramework targetFramework) {
      var frameworkSpecificGroup = nupkg.GetReferenceItems().GetNearest(targetFramework);
      if (frameworkSpecificGroup == null) {
        return None<Tuple<string, Version>>();
      }
      return Tuple.Create(nupkg.GetIdentity().Id,
                          frameworkSpecificGroup.TargetFramework.Version);
    }

    private static ICollection<string> FindAssemblies(PackageReaderBase nupkg,
                                                      PackageReference packageReference,
                                                      string packagesDir,
                                                      INuspecCoreReader nuspec) {
      var referenceItems = nupkg.GetReferenceItems()
                                .GetNearest(packageReference.TargetFramework)?
                                .Items ?? Enumerable.Empty<string>();
      return referenceItems.Select(path => PathInPackage(packagesDir, nuspec, path))
                           .ToList();
    }

    private static string PathInPackage(string packagesDir,
                                        INuspecCoreReader nuspec,
                                        string path)
      => Path.Combine(packagesDir, $"{nuspec.GetId()}.{nuspec.GetVersion()}", path);

    private static IEnumerable<Tuple<string, Version>>
      FindFrameworkAssemblyReferences(NuspecReader nuspec,
                                      PackageReference packageReference) {
      var group = nuspec.GetFrameworkReferenceGroups()
                        .GetNearest(packageReference.TargetFramework);
      return group?.Items
                   .Select(assemblyName =>
                           Tuple.Create(assemblyName,
                                        group.TargetFramework.Version))
             ?? Enumerable.Empty<Tuple<string, Version>>();
    }

    private static LocalPackageInfo FindBestMatch(NuGetv3LocalRepository packageRepository,
                                                  PackageReference packageReference)
      => packageRepository
        .FindPackagesById(packageReference.PackageIdentity.Id)
        .FindBestMatch(new VersionRange(packageReference.PackageIdentity.Version), info => info?.Version);

    private static void ExecuteNuGet(string arguments) {
      var process = new Process();
      process.StartInfo = new ProcessStartInfo("nuget", arguments) {
        CreateNoWindow = true,
        UseShellExecute = false
      };
      process.Start();
      process.WaitForExit();
    }

    private static IEnumerable<PackageReference> PackageReferencesFromFile(string s)
      => File.Exists(s) ?
           new PackagesConfigReader(File.OpenRead(s)).GetPackages() :
           Enumerable.Empty<PackageReference>();

    private static Optional<string> FindFrameworkAssembly(KeyValuePair<string, Version> reference)
      => ResolveFrameworkAssembly(reference.Key, reference.Value);
  }
}