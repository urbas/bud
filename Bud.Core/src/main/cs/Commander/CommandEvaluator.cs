using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bud.Commander {
  public static class CommandEvaluator {
    public static string EvaluateToJsonSynchronously(IContext context, string command) {
      try {
        return JsonConvert.SerializeObject(EvaluateSync(context, command));
      } catch (Exception ex) {
        throw new OperationCanceledException(string.Format("Task '{0}' failed.", command), ex);
      }
    }

    public static object EvaluateSync(IContext context, string command) {
      return context.EvaluateKeySync(Key.Parse(command));
    }
  }
}