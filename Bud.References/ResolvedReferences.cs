using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bud.References {
  public class ResolvedReferences {
    public static readonly ResolvedReferences Empty
      = new ResolvedReferences(ImmutableList<Assembly>.Empty,
                               ImmutableList<FrameworkAssembly>.Empty);

    public IImmutableList<Assembly> Assemblies { get; }
    public IImmutableList<FrameworkAssembly> FrameworkAssemblies { get; }

    public ResolvedReferences(IEnumerable<Assembly> assemblies,
                              IEnumerable<FrameworkAssembly> frameworkAssemblies) {
      Assemblies = assemblies as IImmutableList<Assembly> ?? assemblies.ToImmutableList();
      FrameworkAssemblies = frameworkAssemblies as IImmutableList<FrameworkAssembly> ?? frameworkAssemblies.ToImmutableList();
    }

    protected bool Equals(ResolvedReferences other)
      => Assemblies.SequenceEqual(other.Assemblies) &&
         FrameworkAssemblies.SequenceEqual(other.FrameworkAssemblies);

    public override bool Equals(object obj)
      => !ReferenceEquals(null, obj) &&
         (ReferenceEquals(this, obj) ||
          obj.GetType() == GetType() && Equals((ResolvedReferences) obj));

    public override int GetHashCode() {
      unchecked {
        return (Assemblies.GetHashCode()*397) ^ FrameworkAssemblies.GetHashCode();
      }
    }

    public ResolvedReferences Add(ResolvedReferences resolvedReferences) {
      return new ResolvedReferences(Assemblies.Concat(resolvedReferences.Assemblies).ToImmutableList(),
                                    FrameworkAssemblies.Concat(resolvedReferences.FrameworkAssemblies).ToImmutableList());
    }
  }
}