using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.Scripting {
  public class BudReferences : IAssemblyReferences {
    public IReadOnlyDictionary<string, string> Get() => LazyReferencesInitializer.BudReferences;

    private static KeyValuePair<string, string> ToAssemblyNamePath(Type typ) {
      var assembly = typ.Assembly;
      return new KeyValuePair<string, string>(assembly.GetName().Name,
                                              assembly.Location);
    }

    private static class LazyReferencesInitializer {
      public static readonly IReadOnlyDictionary<string, string> BudReferences = new[] {
        ToAssemblyNamePath(typeof(Option)),
        ToAssemblyNamePath(typeof(BatchExec)),
      }.ToImmutableDictionary();
    }
  }
}