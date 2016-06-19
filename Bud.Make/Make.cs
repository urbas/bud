﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.Building;

namespace Bud.Make {
  public static class Make {
    public static Rule Rule(string output,
                            Action<string, string> recipe,
                            string input)
      => new Rule(output,
                  (inputFiles, outputFile) => recipe(inputFiles[0], outputFile),
                  ImmutableArray.Create(input));

    public static Rule Rule(string outputFile,
                            Action<ImmutableArray<string>, string> recipe,
                            params string[] inputFiles)
      => new Rule(outputFile, recipe, ImmutableArray.CreateRange(inputFiles));

    /// <summary>
    ///   Executes rule <paramref name="ruleToBuild" /> as defined in <paramref name="rules" />.
    ///   This method executes the rules in a single thread synchronously.
    /// </summary>
    public static void Execute(IEnumerable<Rule> rules, string ruleToBuild, Option<string> workingDir = default(Option<string>)) {
      var workingDirReal = workingDir.GetOrElse(Directory.GetCurrentDirectory);
      var rulesDictionary = new Dictionary<string, Rule>();
      foreach (var r in rules) {
        if (rulesDictionary.ContainsKey(r.Output)) {
          throw new Exception($"Found a duplicate rule '{r.Output}'.");
        }
        rulesDictionary.Add(r.Output, r);
      }
      var ruleOptional = rulesDictionary.Get(ruleToBuild);
      if (!ruleOptional.HasValue) {
        throw new Exception($"Could not find rule '{ruleToBuild}'.");
      }
      var rule = ruleOptional.Value;
      InvokeRecipe(workingDirReal, rulesDictionary, rule, new HashSet<string>(), new List<string>());
    }

    private static void InvokeRecipe(string workingDir,
                                     IReadOnlyDictionary<string, Rule> rulesDictionary,
                                     Rule rule,
                                     ISet<string> alreadyInvokedRules,
                                     IList<string> currentlyExecutingRules) {
      if (currentlyExecutingRules.Contains(rule.Output)) {
        throw new Exception($"Detected a cycle in rule dependencies: '{rule.Output} -> {string.Join(" -> ", currentlyExecutingRules.Reverse())}'.");
      }
      if (alreadyInvokedRules.Contains(rule.Output)) {
        return;
      }
      currentlyExecutingRules.Add(rule.Output);
      foreach (var dependentRule in rule.Inputs.Gather(rulesDictionary.Get)) {
        InvokeRecipe(workingDir, rulesDictionary, dependentRule, alreadyInvokedRules, currentlyExecutingRules);
      }
      var inputAbsPaths = rule.Inputs
                              .Select(input => Path.Combine(workingDir, input))
                              .ToImmutableArray();
      TimestampBasedBuilder.Build(rule.Recipe,
                                  inputAbsPaths,
                                  Path.Combine(workingDir, rule.Output));
      alreadyInvokedRules.Add(rule.Output);
      currentlyExecutingRules.RemoveAt(currentlyExecutingRules.Count - 1);
    }
  }
}