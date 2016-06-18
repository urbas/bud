using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bud.Make {
  public class Rule {
    private readonly Action<IReadOnlyList<string>, string> recipe;
    public IReadOnlyList<string> Inputs { get; }
    public string Output { get; }

    public Rule(string output,
                Action<IReadOnlyList<string>, string> recipe,
                IReadOnlyList<string> inputs) {
      this.recipe = recipe;
      Inputs = inputs;
      Output = output;
    }

    public void Recipe(IReadOnlyList<string> inputFiles, string outputFile)
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