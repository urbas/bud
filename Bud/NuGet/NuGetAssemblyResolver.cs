using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Repositories;
using NuGet.Versioning;

namespace Bud.NuGet {
  public class NuGetAssemblyResolver : IAssemblyResolver {
    public ResolvedAssemblies ResolveAssemblies(IEnumerable<string> packagesConfigFiles,
                                                string outputDirectory) {
      var packagesDir = Path.Combine(outputDirectory, "packages");
      foreach (var packagesConfigFile in packagesConfigFiles) {
        ExecuteNuGet($"restore {packagesConfigFile} -PackagesDirectory {packagesDir}");
      }
      var packagesV3Dir = Path.Combine(outputDirectory, "packages-v3");
      ExecuteNuGet($"init {packagesDir} {packagesV3Dir}");
      var packageReferences = packagesConfigFiles.SelectMany(PackageReferencesFromFile);
      var packageRepository = new NuGetv3LocalRepository(packagesV3Dir, true);
      var assemblies = new List<string>();
      var frameworkReferences = new List<string>();
      foreach (var packageReference in packageReferences) {
        var packageInfo = FindBestMatch(packageRepository, packageReference);
        var nuspec = new NuspecReader(XDocument.Load(packageInfo.ManifestPath));
        frameworkReferences.AddRange(FindFrameworkAssemblies(nuspec, packageReference));

        using (var fileStream = File.OpenRead(packageInfo.ZipPath)) {
          var nupkg = new PackageReader(fileStream, false);
          var packageAssemblies = FindAssemblies(nupkg, packageReference, packagesDir, nuspec);
          assemblies.AddRange(packageAssemblies);
        }
      }
      return new ResolvedAssemblies(frameworkReferences.ToImmutableHashSet(),
                                    assemblies.ToImmutableHashSet());
    }

    private static IEnumerable<string> FindAssemblies(PackageReader nupkg,
                                                      PackageReference packageReference,
                                                      string packagesDir, NuspecReader nuspec) {
      var referenceItems = nupkg.GetReferenceItems()
                                .GetNearest(packageReference.TargetFramework)?
                                .Items ?? Enumerable.Empty<string>();
      return referenceItems.Select(path => PathInPackage(packagesDir, nuspec, path));
    }

    private static string PathInPackage(string packagesDir,
                                        NuspecReader nuspec,
                                        string path)
      => Path.Combine(packagesDir, $"{nuspec.GetId()}.{nuspec.GetVersion()}", path);

    private static IEnumerable<string> FindFrameworkAssemblies(NuspecReader nuspec,
                                                               PackageReference packageReference)
      => nuspec.GetFrameworkReferenceGroups()
               .GetNearest(packageReference.TargetFramework)?
               .Items ?? Enumerable.Empty<string>();

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