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
                        IEnumerable<string> assemblies)
      : this(Files.ToTimestampedFiles(sources).ToImmutableArray(),
             Files.ToTimestampedFiles(assemblies).ToImmutableArray()) {}

    public CompileInput(ImmutableArray<Timestamped<string>> sources,
                        ImmutableArray<Timestamped<string>> assemblies) {
      Sources = sources;
      Assemblies = assemblies;
      cachedHashCode = new Lazy<int>(ComputeHashCode);
    }

    public ImmutableArray<Timestamped<string>> Sources { get; }
    public ImmutableArray<Timestamped<string>> Assemblies { get; }

    public bool Equals(CompileInput other)
      => Sources.SequenceEqual(other.Sources) &&
         Assemblies.SequenceEqual(other.Assemblies);

    public override bool Equals(object obj)
      => !ReferenceEquals(null, obj) &&
         obj is CompileInput &&
         Equals((CompileInput) obj);

    public override int GetHashCode() => cachedHashCode.Value;

    public static bool operator ==(CompileInput left, CompileInput right) => left.Equals(right);

    public static bool operator !=(CompileInput left, CompileInput right) => !left.Equals(right);

    private int ComputeHashCode() {
      unchecked {
        return ElementwiseHashCode(Sources) * 397 ^ ElementwiseHashCode(Assemblies);
      }
    }

    public static CompileInput FromInOut(InOut input) => FromFiles(input.Files.Select(file => file.Path));

    public static CompileInput FromFiles(IEnumerable<string> inputFiles) {
      var sources = new List<string>();
      var dependencies = new List<string>();
      foreach (var inputFile in inputFiles) {
        if (inputFile.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase)) {
          sources.Add(inputFile);
        } else {
          dependencies.Add(inputFile);
        }
      }
      return new CompileInput(sources, dependencies);
    }
  }
}