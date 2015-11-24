using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.IO;
using static Bud.Collections.EnumerableUtils;

namespace Bud.Cs {
  public class CompileInput {
    private readonly Lazy<int> cachedHashCode;

    public CompileInput(IEnumerable<string> sources,
                        IEnumerable<IAssemblyReference> assemblies,
                        IEnumerable<CompileOutput> dependencies)
      : this(Files.ToTimestampedFiles(sources),
             ToTimestampedAssemblyReferences(assemblies),
             dependencies.ToImmutableArray()) {}

    public CompileInput(ImmutableArray<Timestamped<string>> sources,
                        ImmutableArray<Timestamped<IAssemblyReference>> assemblies,
                        ImmutableArray<CompileOutput> dependencies) {
      Dependencies = dependencies;
      Sources = sources;
      Assemblies = assemblies;
      cachedHashCode = new Lazy<int>(ComputeHashCode);
    }

    public static ImmutableArray<Timestamped<IAssemblyReference>> ToTimestampedAssemblyReferences(IEnumerable<IAssemblyReference> assemblies)
      => assemblies.Select(reference => reference.ToTimestamped()).ToImmutableArray();

    public ImmutableArray<Timestamped<string>> Sources { get; }
    public ImmutableArray<Timestamped<IAssemblyReference>> Assemblies { get; }
    public ImmutableArray<CompileOutput> Dependencies { get; }

    public bool Equals(CompileInput other)
      => Sources.SequenceEqual(other.Sources) &&
         Assemblies.SequenceEqual(other.Assemblies) &&
         Dependencies.SequenceEqual(other.Dependencies);

    public override bool Equals(object obj)
      => !ReferenceEquals(null, obj) &&
         obj is CompileInput &&
         Equals((CompileInput) obj);

    public override int GetHashCode() => cachedHashCode.Value;

    public static bool operator ==(CompileInput left, CompileInput right) => left.Equals(right);

    public static bool operator !=(CompileInput left, CompileInput right) => !left.Equals(right);

    private int ComputeHashCode() {
      unchecked {
        var hashCode = ElementwiseHashCode(Dependencies) * 397;
        hashCode = (hashCode ^ ElementwiseHashCode(Sources)) * 397;
        return hashCode ^ ElementwiseHashCode(Assemblies);
      }
    }
  }
}