using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.Scripting {
  public class BudReferences : IAssemblyReferences {
    public IReadOnlyDictionary<string, string> Get()
      => ImmutableDictionary<string, string>
        .Empty
        .Add(typeof(Option).Assembly.GetName().Name, typeof(Option).Assembly.Location);
  }
}