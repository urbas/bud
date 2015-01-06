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
    public static object EvaluateSynchronously(IContext context, string command) {
      try {
        var evaluationOutput = Evaluate(context, command);
        evaluationOutput.Wait();
        return evaluationOutput.Result;
      } catch (Exception ex) {
        throw new OperationCanceledException(string.Format("Evaluation of '{0}' did not complete successfully.", command), ex);
      }
    }

    public static async Task<object> Evaluate(IContext context, string command) {
      Key keyToEvaluate = Key.Parse(command);
      await context.EvaluateKey(keyToEvaluate);
      return context.GetOutputOf(keyToEvaluate);
    }
  }
}

