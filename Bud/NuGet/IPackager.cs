using System.Collections.Generic;
using Bud.IO;

namespace Bud.NuGet {
  public interface IPackager {
    string Pack(string outputDir, string baseDir, string packageId, string version, IEnumerable<PackageFile> files, IEnumerable<PackageDependency> packageDependencies, NuGetPackageMetadata packageMetadata);
  }
}