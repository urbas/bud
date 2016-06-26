using System;
using System.Collections.Immutable;
using System.Linq;

namespace Bud {
  public class Rule {
    private readonly Action<ImmutableArray<string>, string> recipe;
    public ImmutableArray<string> Inputs { get; }
    public string Output { get; }

    public Rule(string output,
                Action<ImmutableArray<string>, string> recipe,
                ImmutableArray<string> inputs) {
      this.recipe = recipe;
      Inputs = inputs;
      Output = output;
    }

    public void Recipe(ImmutableArray<string> inputFiles, string outputFile)
      => recipe(inputFiles, outputFile);

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