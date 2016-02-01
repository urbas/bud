using System.Collections.Generic;

namespace Bud.NuGet {
  public interface IPackageResolver {
    IEnumerable<string> Resolve(IEnumerable<PackageReference> packageReferences,
                                string packagesDir,
                                string cacheDir);
  }
}