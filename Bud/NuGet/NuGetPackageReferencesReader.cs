using System.Collections.Generic;
using System.IO;
using NuGet.Packaging;
using static System.IO.File;
using static System.Linq.Enumerable;

namespace Bud.NuGet {
  public static class NuGetPackageReferencesReader {
    public static IEnumerable<PackageReference> LoadReferences(Stream packageConfigXmlStream)
      => new PackagesConfigReader(packageConfigXmlStream)
        .GetPackages()
        .Select(ToPackageReference);


    public static IEnumerable<PackageReference> LoadReferences(IEnumerable<string> packageConfigFiles)
      => packageConfigFiles.SelectMany(LoadReferences);


    public static IEnumerable<PackageReference> LoadReferences(string file)
      => Exists(file) ?
           LoadReferences(OpenRead(file)) :
           Empty<PackageReference>();

    private static PackageReference ToPackageReference(global::NuGet.Packaging.PackageReference reference)
      => new PackageReference(reference.PackageIdentity.Id,
                              reference.PackageIdentity.Version,
                              reference.TargetFramework);
  }
}