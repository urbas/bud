using System.Collections.Generic;

namespace Bud.Scripting {
  public interface IAssemblyPaths {
    IReadOnlyDictionary<string, string> Get();
  }
}