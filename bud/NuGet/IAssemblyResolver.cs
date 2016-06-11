using System.Collections.Generic;

namespace Bud.NuGet {
  public interface IAssemblyResolver {
    IEnumerable<string> FindAssemblies(IEnumerable<PackageReference> packageReferences,
                                       string packagesCacheDir,
                                       string scratchDir);
  }
}