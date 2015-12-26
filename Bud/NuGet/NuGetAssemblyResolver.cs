using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging;

namespace Bud.NuGet {
  // TODO: Implement :)
  public class NuGetAssemblyResolver : IAssemblyResolver {
    public IEnumerable<string> ResolveAssemblies(IEnumerable<PackageReference> packageReferences)
      => Enumerable.Empty<string>();
  }
}