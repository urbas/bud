using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.IO;
using static Bud.Collections.EnumerableUtils;

namespace Bud.Compilation {
  public struct CSharpCompilationInput {
    public CSharpCompilationInput(IEnumerable<string> sources,
                                  IEnumerable<AssemblyReference> assemblies,
                                  IEnumerable<CSharpCompilationOutput> cSharpCompilationOutputs) {
      Dependencies = cSharpCompilationOutputs.ToImmutableArray();
      Sources = sources.Select(Files.ToTimeHashedFile).ToImmutableArray();
      Assemblies = assemblies.Select(reference => reference.ToHashed()).ToImmutableArray();
    }

    public ImmutableArray<Hashed<string>> Sources { get; }
    public ImmutableArray<Hashed<AssemblyReference>> Assemblies { get; }
    public ImmutableArray<CSharpCompilationOutput> Dependencies { get; }

    public bool Equals(CSharpCompilationInput other)
      => Sources.SequenceEqual(other.Sources) &&
         Assemblies.SequenceEqual(other.Assemblies) &&
         Dependencies.SequenceEqual(other.Dependencies);

    public override bool Equals(object obj)
      => !ReferenceEquals(null, obj) &&
         obj is CSharpCompilationInput &&
         Equals((CSharpCompilationInput) obj);

    public override int GetHashCode() {
      unchecked {
        var hashCode = ElementwiseHashCode(Dependencies) * 397;
        hashCode = (hashCode ^ ElementwiseHashCode(Sources)) * 397;
        return hashCode ^ ElementwiseHashCode(Assemblies);
      }
    }

    public static bool operator ==(CSharpCompilationInput left, CSharpCompilationInput right) => left.Equals(right);
    public static bool operator !=(CSharpCompilationInput left, CSharpCompilationInput right) => !left.Equals(right);
  }
}