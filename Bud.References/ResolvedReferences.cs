using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.References {
  public class ResolvedReferences {
    public static readonly ResolvedReferences Empty
      = new ResolvedReferences(ImmutableList<ResolvedAssembly>.Empty,
                               ImmutableList<FrameworkAssemblyReference>.Empty);

    public IEnumerable<ResolvedAssembly> Assemblies { get; }
    public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies { get; }

    public ResolvedReferences(IEnumerable<ResolvedAssembly> assemblies,
                              IEnumerable<FrameworkAssemblyReference> frameworkAssemblies) {
      Assemblies = assemblies;
      FrameworkAssemblies = frameworkAssemblies;
    }
  }
}