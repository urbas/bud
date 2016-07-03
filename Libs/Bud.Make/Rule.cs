using System.Collections.Immutable;
using System.Linq;
using Bud.Building;

namespace Bud {
  public class Rule {
    public ImmutableArray<string> Inputs { get; }
    public FilesBuilder Recipe { get; }
    public string Output { get; }

    public Rule(string output,
                FilesBuilder recipe,
                ImmutableArray<string> inputs) {
      Recipe = recipe;
      Inputs = inputs;
      Output = output;
    }

    protected bool Equals(Rule other)
      => Inputs.SequenceEqual(other.Inputs) &&
         string.Equals(Output, other.Output);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == GetType() && Equals((Rule) obj);
    }

    public override int GetHashCode() {
      unchecked {
        return (Inputs.GetHashCode()*397) ^ Output.GetHashCode();
      }
    }
  }
}