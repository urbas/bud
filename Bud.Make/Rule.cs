using System;
using System.Collections.Generic;

namespace Bud.Make {
  public class Rule {
    public readonly Action<IReadOnlyList<string>, string> Recipe;
    public readonly IReadOnlyList<string> Inputs;
    public readonly string Output;

    public Rule(string output,
                Action<IReadOnlyList<string>, string> recipe,
                IReadOnlyList<string> inputs) {
      Recipe = recipe;
      Inputs = inputs;
      Output = output;
    }
  }
}