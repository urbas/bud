using System.Collections.Generic;

namespace Bud.NuGet {
  public interface IAssemblyResolver {
    IEnumerable<string> ResolveAssemblies(IEnumerable<string> packagesConfigFiles);
  }
}