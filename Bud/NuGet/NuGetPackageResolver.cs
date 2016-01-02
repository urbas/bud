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
using static Bud.Util.Optional;

namespace Bud.NuGet {
  public class NuGetPackageResolver : IPackageResolver {
    public IEnumerable<string> Resolve(IEnumerable<string> packagesConfigFiles,
                                       string cacheDir) {
      var packagesDir = Path.Combine(cacheDir, "packages");
      foreach (var packagesConfigFile in packagesConfigFiles) {
        ExecuteNuGet($"restore {packagesConfigFile} -PackagesDirectory {packagesDir}");
      }
      var packagesV3Dir = Path.Combine(cacheDir, "packages-index");
      ExecuteNuGet($"init {packagesDir} {packagesV3Dir}");
      var packageReferences = packagesConfigFiles.SelectMany(PackageReferencesFromFile);
      var packageRepository = new NuGetv3LocalRepository(packagesV3Dir, true);
      var frameworkAssemblies = new List<FrameworkAssemblyReference>();
      var assemblies = new List<string>();
      foreach (var packageReference in packageReferences) {
        var packageInfo = FindBestMatch(packageRepository, packageReference);
        var nuspec = new NuspecReader(XDocument.Load(packageInfo.ManifestPath));

        frameworkAssemblies.AddRange(FindFrameworkAssemblyReferences(nuspec, packageReference));

        using (var fileStream = File.OpenRead(packageInfo.ZipPath)) {
          var nupkg = new PackageReader(fileStream, false);
          var packageAssemblies = FindAssemblies(nupkg, packageReference, packagesDir, nuspec);
          if (packageAssemblies.Any()) {
            assemblies.AddRange(packageAssemblies);
          } else {
            var reference = ToFrameworkAssemblyReference(nupkg, packageReference.TargetFramework);
            if (reference.HasValue) {
              frameworkAssemblies.Add(reference.Value);
            }
          }
        }
      }

      Console.WriteLine($"Framework assemblies:\n{string.Join("\n", frameworkAssemblies)}");
      assemblies.AddRange(FrameworkAssemblyResolver.ResolveFrameworkAsseblies(frameworkAssemblies));

      return assemblies;
    }

    private static Optional<FrameworkAssemblyReference> ToFrameworkAssemblyReference(PackageReader nupkg, NuGetFramework targetFramework) {
      var frameworkSpecificGroup = nupkg.GetReferenceItems().GetNearest(targetFramework);
      if (frameworkSpecificGroup == null) {
        return None<FrameworkAssemblyReference>();
      }
      return new FrameworkAssemblyReference(nupkg.GetIdentity().Id,
                                            frameworkSpecificGroup.TargetFramework);
    }

    private static IEnumerable<string> FindAssemblies(PackageReaderBase nupkg,
                                                      PackageReference packageReference,
                                                      string packagesDir,
                                                      NuspecReader nuspec) {
      var referenceItems = nupkg.GetReferenceItems()
                                .GetNearest(packageReference.TargetFramework)?
                                .Items ?? Enumerable.Empty<string>();
      return referenceItems.Select(path => PathInPackage(packagesDir, nuspec, path));
    }

    private static string PathInPackage(string packagesDir,
                                        INuspecCoreReader nuspec,
                                        string path)
      => Path.Combine(packagesDir, $"{nuspec.GetId()}.{nuspec.GetVersion()}", path);

    private static IEnumerable<FrameworkAssemblyReference>
      FindFrameworkAssemblyReferences(NuspecReader nuspec,
                                      PackageReference packageReference) {
      var group = nuspec.GetFrameworkReferenceGroups()
                        .GetNearest(packageReference.TargetFramework);
      return group?.Items
                   .Select(assemblyName => new FrameworkAssemblyReference(assemblyName, group.TargetFramework))
             ?? Enumerable.Empty<FrameworkAssemblyReference>();
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
  }
}