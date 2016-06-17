using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using Bud.Building;
using Bud.References;
using ReflectionAssembly = System.Reflection.Assembly;

namespace Bud.Scripting {
  public class BudReferenceResolver : IReferenceResolver {
    public readonly ImmutableDictionary<string, Option<string>> NoReferences
      = ImmutableDictionary<string, Option<string>>.Empty;

    public IDictionary<string, Option<string>> Resolve(IEnumerable<string> references) {
      var resolvedReferences = new Dictionary<string, Option<string>>();
      foreach (var reference in references) {
        var assembly = LazyReferencesInitializer.BudReferences.Get(reference);
        if (assembly.HasValue) {
          AddReference(reference, assembly.Value, resolvedReferences);
        } else if (File.Exists(reference)) {
          resolvedReferences.Add(Path.GetFileNameWithoutExtension(reference), reference);
        } else {
          resolvedReferences.Add(reference, Option.None<string>());
        }
      }
      return new ReadOnlyDictionary<string, Option<string>>(resolvedReferences);
    }

    private static void AddReference(string assemblyName,
                                     ReflectionAssembly assembly,
                                     IDictionary<string, Option<string>> resolvedReferences) {
      if (resolvedReferences.ContainsKey(assemblyName)) {
        return;
      }
      resolvedReferences.Add(assemblyName, assembly.Location);
      foreach (var referencedAssembly in assembly.GetReferencedAssemblies()) {
        var budAssembly = LazyReferencesInitializer.BudReferences.Get(referencedAssembly.Name);
        if (budAssembly.HasValue) {
          AddReference(referencedAssembly.Name, budAssembly.Value, resolvedReferences);
        } else if (!resolvedReferences.ContainsKey(referencedAssembly.Name)) {
          resolvedReferences.Add(referencedAssembly.Name, Option.None<string>());
        }
      }
    }

    private static class LazyReferencesInitializer {
      public static readonly IReadOnlyDictionary<string, ReflectionAssembly> BudReferences = new[] {
        ToAssemblyNamePath(typeof(Option)),
        ToAssemblyNamePath(typeof(BatchExec)),
        ToAssemblyNamePath(typeof(Make.Make)),
        ToAssemblyNamePath(typeof(HashBasedBuilder)),
      }.ToImmutableDictionary();
    }

    private static KeyValuePair<string, ReflectionAssembly> ToAssemblyNamePath(Type typ) {
      var assembly = typ.Assembly;
      return new KeyValuePair<string, ReflectionAssembly>(assembly.GetName().Name,
                                                          assembly);
    }
  }
}