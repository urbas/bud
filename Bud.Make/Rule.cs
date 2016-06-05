using System;

namespace Bud.Make {
  public class Rule {
    public readonly Action<string, string> Recipe;
    public readonly string Input;
    public readonly string Output;

    public Rule(string input, string output, Action<string, string> recipe) {
      Recipe = recipe;
      Input = input;
      Output = output;
    }
  }
}