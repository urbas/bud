using System.Collections.Generic;
using System.Linq;

namespace Bud.NuGet {
  // TODO: Implement :)
  public class NuGetAssemblyResolver : IAssemblyResolver {
    public IEnumerable<string> ResolveAssemblies(IEnumerable<string> packagesConfigFiles)
      => Enumerable.Empty<string>();
  }
}