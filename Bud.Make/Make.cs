using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Bud.Building;

namespace Bud.Make {
  public static class Make {
    public static Rule Rule(string output, string input, Action<string, string> recipe)
      => new Rule(input, output, recipe);

    public static void Execute(string[] args, params Rule[] rules)
      => Execute(args, Directory.GetCurrentDirectory(), rules);

    public static void Execute(string[] args, string workingDir, IEnumerable<Rule> rules) {
      foreach (var rule in rules) {
        TimestampBasedBuilder.Build((inputFiles, outputFile) => rule.Recipe(inputFiles[0], outputFile),
                                    new ReadOnlyCollection<string>(new[] {Path.Combine(workingDir, rule.Input)}), Path.Combine(workingDir, rule.Output));
      }
    }
  }
}