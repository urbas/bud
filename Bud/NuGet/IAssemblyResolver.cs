using System.Collections.Generic;

namespace Bud.NuGet {
  public interface IAssemblyResolver {
    ResolvedAssemblies ResolveAssemblies(IEnumerable<string> packagesConfigFiles, string outputDirectory);
  }
}