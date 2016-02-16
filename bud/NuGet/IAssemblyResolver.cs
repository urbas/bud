using System.Collections.Generic;

namespace Bud.NuGet {
  public interface IAssemblyResolver {
    IEnumerable<string> FindAssembly(IEnumerable<PackageReference> packageReferences,
                                     string packagesDir,
                                     string cacheDir);
  }
}