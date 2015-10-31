using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.IO;
using static Bud.Collections.EnumerableUtils;

namespace Bud.Cs {
  public struct CompileInput {
    public CompileInput(IEnumerable<string> sources,
                                  IEnumerable<AssemblyReference> assemblies,
                                  IEnumerable<CompileOutput> cSharpCompilationOutputs) {
      Dependencies = cSharpCompilationOutputs.ToImmutableArray();
      Sources = sources.Select(Files.ToTimeHashedFile).ToImmutableArray();
      Assemblies = assemblies.Select(reference => reference.ToHashed()).ToImmutableArray();
    }

    public ImmutableArray<Hashed<string>> Sources { get; }
    public ImmutableArray<Hashed<AssemblyReference>> Assemblies { get; }
    public ImmutableArray<CompileOutput> Dependencies { get; }

    public bool Equals(CompileInput other)
      => Sources.SequenceEqual(other.Sources) &&
         Assemblies.SequenceEqual(other.Assemblies) &&
         Dependencies.SequenceEqual(other.Dependencies);

    public override bool Equals(object obj)
      => !ReferenceEquals(null, obj) &&
         obj is CompileInput &&
         Equals((CompileInput) obj);

    public override int GetHashCode() {
      unchecked {
        var hashCode = ElementwiseHashCode(Dependencies) * 397;
        hashCode = (hashCode ^ ElementwiseHashCode(Sources)) * 397;
        return hashCode ^ ElementwiseHashCode(Assemblies);
      }
    }

    public static bool operator ==(CompileInput left, CompileInput right) => left.Equals(right);
    public static bool operator !=(CompileInput left, CompileInput right) => !left.Equals(right);
  }
}