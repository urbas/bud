using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.References {
  public class ResolvedReferences {
    public static readonly ResolvedReferences Empty
      = new ResolvedReferences(ImmutableList<Assembly>.Empty,
                               ImmutableList<FrameworkAssembly>.Empty);

    public IEnumerable<Assembly> Assemblies { get; }
    public IEnumerable<FrameworkAssembly> FrameworkAssemblies { get; }

    public ResolvedReferences(IEnumerable<Assembly> assemblies,
                              IEnumerable<FrameworkAssembly> frameworkAssemblies) {
      Assemblies = assemblies;
      FrameworkAssemblies = frameworkAssemblies;
    }
  }
}