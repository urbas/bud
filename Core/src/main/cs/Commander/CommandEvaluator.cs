using System;
using System.Threading.Tasks;

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