using System.Collections.Generic;
using NuGet.Packaging;

namespace Bud.NuGet {
  public interface IAssemblyResolver {
    IEnumerable<string> ResolveAssemblies(IEnumerable<PackageReference> packageReferences);
  }
}