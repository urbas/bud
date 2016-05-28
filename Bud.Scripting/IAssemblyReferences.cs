using System.Collections.Generic;

namespace Bud.Scripting {
  public interface IAssemblyReferences {
    IReadOnlyDictionary<string, string> Get();
  }
}