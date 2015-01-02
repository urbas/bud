using System;
using System.Linq;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using Bud.Plugins.CSharp;
using Bud.Plugins.Build;
using System.IO;
using System.Collections.Generic;
using Bud;
using System.Threading.Tasks;
using System.Reflection;
using Bud.Commander;

namespace Bud.Commander {
  public static class CommandEvaluator {
    public static object Evaluate(Settings settings, string command) {
      Key keyToEvaluate = Key.Parse(command);
      var evaluationContext = EvaluationContext.FromSettings(settings);
      var evaluationResult = evaluationContext
        .EvaluateKey(keyToEvaluate)
        .ContinueWith(t => evaluationContext.GetOutputOf(keyToEvaluate));
      evaluationResult.Wait();
      return evaluationResult.Result;
    }
  }
}

