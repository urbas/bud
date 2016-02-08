using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Bud.Collections.EnumerableUtils;

namespace Bud.Cs {
  public class CompileInput {
    public IEnumerable<string> Sources { get; }
    public IEnumerable<CompileOutput> Dependencies { get; }
    public IEnumerable<string> Assemblies { get; }

    public CompileInput(IEnumerable<string> sources,
                        IEnumerable<CompileOutput> dependencies,
                        IEnumerable<string> assemblies) {
      Sources = sources;
      Dependencies = dependencies;
      Assemblies = assemblies;
    }

    protected bool Equals(CompileInput other)
      => Sources.SequenceEqual(other.Sources) &&
         Dependencies.SequenceEqual(other.Dependencies) &&
         Assemblies.SequenceEqual(other.Assemblies);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == GetType() && Equals((CompileInput) obj);
    }

    public override int GetHashCode() {
      unchecked {
        var hashCode = ElementwiseHashCode(Sources);
        hashCode = (hashCode*397) ^ ElementwiseHashCode(Dependencies);
        hashCode = (hashCode*397) ^ ElementwiseHashCode(Assemblies);
        return hashCode;
      }
    }

    public static CompileInput Create(IEnumerable<string> sources,
                                      IEnumerable<CompileOutput> dependencies,
                                      IImmutableList<string> assemblyReferences)
      => new CompileInput(sources, dependencies, assemblyReferences);
  }
}