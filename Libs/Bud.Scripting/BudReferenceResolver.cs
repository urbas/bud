using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.Building;
using Bud.References;
using ReflectionAssembly = System.Reflection.Assembly;

namespace Bud.Scripting {
  public class BudReferenceResolver : IReferenceResolver {
    public readonly ImmutableDictionary<string, Option<string>> NoReferences
      = ImmutableDictionary<string, Option<string>>.Empty;

    public ResolvedReferences Resolve(IEnumerable<string> references) {
      var assemblies = new Dictionary<string, string>();
      var frameworkAssemblies = new HashSet<string>();
      Resolve(references, assemblies, frameworkAssemblies);
      return new ResolvedReferences(assemblies.Select(assemblyNamePath => new Assembly(assemblyNamePath.Key, assemblyNamePath.Value)).ToImmutableArray(),
                                    frameworkAssemblies.Select(assemblyName => new FrameworkAssembly(assemblyName, FrameworkAssembly.MaxVersion)).ToImmutableArray());
    }

    public void Resolve(IEnumerable<string> references,
                        IDictionary<string, string> assemblies,
                        ISet<string> frameworkAssemblies) {
      foreach (var reference in references) {
        var assemblyOpt = LazyReferencesInitializer.BudReferences.Get(reference);
        if (assemblyOpt.HasValue) {
          if (assemblies.ContainsKey(reference)) {
            continue;
          }
          var assembly = assemblyOpt.Value;
          assemblies.Add(reference, assembly.Location);
          Resolve(assembly.GetReferencedAssemblies().Select(reflectionAssembly => reflectionAssembly.Name), assemblies, frameworkAssemblies);
        } else if (File.Exists(reference)) {
          assemblies.Add(Path.GetFileNameWithoutExtension(reference), reference);
        } else {
          frameworkAssemblies.Add(reference);
        }
      }
    }

    private static class LazyReferencesInitializer {
      public static readonly ImmutableDictionary<string, ReflectionAssembly> BudReferences = new[] {
        ToAssemblyNamePath(typeof(Option)),
        ToAssemblyNamePath(typeof(Exec)),
        ToAssemblyNamePath(typeof(Make)),
        ToAssemblyNamePath(typeof(HashBasedBuilder)),
        ToAssemblyNamePath(typeof(ImmutableArray)),
      }.ToImmutableDictionary();
    }

    private static KeyValuePair<string, ReflectionAssembly> ToAssemblyNamePath(Type typ) {
      var assembly = typ.Assembly;
      return new KeyValuePair<string, ReflectionAssembly>(assembly.GetName().Name,
                                                          assembly);
    }
  }
}