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
    public ResolvedAssemblies ResolveAssemblies(IEnumerable<string> packagesConfigFiles, string outputDirectory) {
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
        var packageInfo = packageRepository
          .FindPackagesById(packageReference.PackageIdentity.Id)
          .FindBestMatch(new VersionRange(packageReference.PackageIdentity.Version), info => info?.Version);
        var nuspec = new NuspecReader(XDocument.Load(packageInfo.ManifestPath));

        var frameworkAssemblies = nuspec.GetFrameworkReferenceGroups()
                                        .GetNearest(packageReference.TargetFramework)?
                                        .Items ?? Enumerable.Empty<string>();
        frameworkReferences.AddRange(frameworkAssemblies);

        using (var fileStream = File.OpenRead(packageInfo.ZipPath)) {
          var nupkg = new PackageReader(fileStream, false);
          var referenceItems = nupkg.GetReferenceItems().GetNearest(packageReference.TargetFramework)?.Items ?? Enumerable.Empty<string>();
          assemblies.AddRange(referenceItems.Select(path => Path.Combine(packagesDir, $"{nuspec.GetId()}.{nuspec.GetVersion()}", path)));
        }
      }
      return new ResolvedAssemblies(frameworkReferences.ToImmutableHashSet(), assemblies.ToImmutableHashSet());
    }

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

  public class ResolvedAssemblies {
    public IImmutableSet<string> FrameworkReferences { get; }
    public IImmutableSet<string> Assemblies { get; }

    public ResolvedAssemblies(IImmutableSet<string> frameworkReferences, IImmutableSet<string> assemblies) {
      FrameworkReferences = frameworkReferences;
      Assemblies = assemblies;
    }
  }
}