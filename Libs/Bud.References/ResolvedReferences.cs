using System.Collections.Immutable;
using System.Linq;

namespace Bud.References {
  public class ResolvedReferences {
    public static readonly ResolvedReferences Empty
      = new ResolvedReferences(ImmutableArray<Assembly>.Empty,
                               ImmutableArray<FrameworkAssembly>.Empty);

    public ImmutableArray<Assembly> Assemblies { get; }
    public ImmutableArray<FrameworkAssembly> FrameworkAssemblies { get; }

    public ResolvedReferences(ImmutableArray<Assembly> assemblies,
                              ImmutableArray<FrameworkAssembly> frameworkAssemblies) {
      Assemblies = assemblies;
      FrameworkAssemblies = frameworkAssemblies;
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
      return new ResolvedReferences(Assemblies.Concat(resolvedReferences.Assemblies).ToImmutableArray(),
                                    FrameworkAssemblies.Concat(resolvedReferences.FrameworkAssemblies).ToImmutableArray());
    }
  }
}