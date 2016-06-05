using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Bud.Building;

namespace Bud.Make {
  public static class Make {
    public static Rule Rule(string output, Action<string, string> recipe, string input)
      => new Rule(output,
                  (inputFiles, outputFile) => recipe(inputFiles[0], outputFile),
                  new ReadOnlyCollection<string>(new[] {input}));

    public static Rule Rule(string outputFile, Action<IReadOnlyList<string>, string> recipe, params string[] inputFiles)
      => new Rule(outputFile, recipe, inputFiles);

    public static void Execute(string[] rulesToBuild, params Rule[] rules)
      => Execute(rulesToBuild, Directory.GetCurrentDirectory(), rules);

    public static void Execute(string[] rulesToBuild,
                               string workingDir,
                               params Rule[] rules)
      => Execute(rulesToBuild, workingDir, (IEnumerable<Rule>) rules);

    public static void Execute(string[] rulesToBuild,
                               string workingDir,
                               IEnumerable<Rule> rules) {
      if (rulesToBuild == null || rulesToBuild.Length == 0) {
        return;
      }
      var rulesDictionary = rules.ToDictionary(r => r.Output, r => r);
      var ruleOptional = rulesDictionary.Get(rulesToBuild[0]);
      if (!ruleOptional.HasValue) {
        throw new Exception($"Could not find rule '{rulesToBuild[0]}'.");
      }
      var rule = ruleOptional.Value;
      InvokeRecipe(workingDir, rulesDictionary, rule);
    }

    private static void InvokeRecipe(string workingDir,
                                     IReadOnlyDictionary<string, Rule> rulesDictionary,
                                     Rule rule) {
      foreach (var dependentRule in rule.Inputs.Gather(rulesDictionary.Get)) {
        InvokeRecipe(workingDir, rulesDictionary, dependentRule);
      }
      TimestampBasedBuilder.Build((inputFiles, outputFile) => rule.Recipe(inputFiles, outputFile),
                                  ToAbsolutePaths(workingDir, rule.Inputs),
                                  Path.Combine(workingDir, rule.Output));
    }

    private static IReadOnlyList<string> ToAbsolutePaths(string workingDir, IEnumerable<string> relativePaths)
      => new ReadOnlyCollection<string>(relativePaths.Select(input => Path.Combine(workingDir, input))
                                                     .ToList());
  }
}