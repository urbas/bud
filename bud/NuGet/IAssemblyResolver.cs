using System.Collections.Generic;

namespace Bud.NuGet {
  public interface IAssemblyResolver {
    IEnumerable<string> FindAssembly(IEnumerable<PackageReference> packageReferences,
                                     string packagesCacheDir,
                                     string scratchDir);
  }
}