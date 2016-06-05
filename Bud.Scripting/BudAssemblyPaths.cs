using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;

namespace Bud.Scripting {
  public class BudAssemblyPaths : IAssemblyPaths {
    public IReadOnlyDictionary<string, string> Get()
      => LazyReferencesInitializer.BudReferences;

    public static Option<string> Get(string assemblyName)
      => LazyReferencesInitializer.BudReferences.Get(assemblyName);

    private static class LazyReferencesInitializer {
      public static readonly IReadOnlyDictionary<string, string> BudReferences = new[] {
        ToAssemblyNamePath(typeof(Option)),
        ToAssemblyNamePath(typeof(BatchExec)),
        ToAssemblyNamePath("Bud.Make"),
        ToAssemblyNamePath("Bud.Building"),
      }.ToImmutableDictionary();
    }

    private static KeyValuePair<string, string> ToAssemblyNamePath(Type typ) {
      var assembly = typ.Assembly;
      return new KeyValuePair<string, string>(assembly.GetName().Name,
                                              assembly.Location);
    }

    private static KeyValuePair<string, string> ToAssemblyNamePath(string budAssemblyName)
      => new KeyValuePair<string, string>(budAssemblyName,
                                          Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), budAssemblyName + ".dll"));
  }
}