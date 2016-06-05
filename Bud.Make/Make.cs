using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Bud.Building;

namespace Bud.Make {
  public static class Make {
    public static Rule Rule(string output, string input, Action<string, string> recipe)
      => new Rule(output, input, recipe);

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

    private static void InvokeRecipe(string workingDir, IReadOnlyDictionary<string, Rule> rulesDictionary, Rule rule) {
      var dependentRule = rulesDictionary.Get(rule.Input);
      if (dependentRule.HasValue) {
        InvokeRecipe(workingDir, rulesDictionary, dependentRule.Value);
      }
      TimestampBasedBuilder.Build((inputFiles, outputFile) => rule.Recipe(inputFiles[0], outputFile),
                                  new ReadOnlyCollection<string>(new[] {Path.Combine(workingDir, rule.Input)}),
                                  Path.Combine(workingDir, rule.Output));
    }
  }
}