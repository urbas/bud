using System.Collections.Generic;
using System.Linq;
using Bud.Collections;

namespace Bud.Cs {
  public class CompileInput {
    public IEnumerable<string> Sources { get; }
    public IEnumerable<CompileOutput> Dependencies { get; }
    public IEnumerable<string> AssemblyReferences { get; }

    public CompileInput(IEnumerable<string> sources, IEnumerable<CompileOutput> dependencies, IEnumerable<string> assemblyReferences) {
      Sources = sources;
      Dependencies = dependencies;
      AssemblyReferences = assemblyReferences;
    }

    protected bool Equals(CompileInput other)
      => Sources.SequenceEqual(other.Sources) &&
                                                 Dependencies.SequenceEqual(other.Dependencies) &&
                                                 AssemblyReferences.SequenceEqual(other.AssemblyReferences);

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
        var hashCode = EnumerableUtils.ElementwiseHashCode(Sources);
        hashCode = (hashCode*397) ^ EnumerableUtils.ElementwiseHashCode(Dependencies);
        hashCode = (hashCode*397) ^ EnumerableUtils.ElementwiseHashCode(AssemblyReferences);
        return hashCode;
      }
    }
  }
}