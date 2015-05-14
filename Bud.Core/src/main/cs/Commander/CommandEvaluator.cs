using System;
using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Bud.Commander {
  internal class CommandEvaluator : ICommandEvaluator {
    public string EvaluateToJsonSync(string command, ref IBuildContext context) {
      try {
        return JsonConvert.SerializeObject(EvaluateSync(context.Context, command));
      } catch (Exception ex) {
        throw new OperationCanceledException(string.Format("Task '{0}' failed.", command), ex);
      }
    }

    public string EvaluateMacroToJsonSync(string macroName, string[] commandLineParameters, ref IBuildContext context) {
      ImmutableDictionary<string, Macro> macros;
      Macro macro;
      if (context.Config.TryEvaluate(Macro.Macros, out macros)
          && macros.TryGetValue(macroName, out macro)) {
        var macroResult = macro.Function(context, commandLineParameters);
        context = macroResult.BuildContext;
        return JsonConvert.SerializeObject(macroResult.Value);
      }
      throw new ArgumentException(string.Format("Could not find macro '{0}'.", macroName));
    }

    public static object EvaluateSync(IContext context, string command) => context.EvaluateKeySync(Key.Parse(command));
  }
}