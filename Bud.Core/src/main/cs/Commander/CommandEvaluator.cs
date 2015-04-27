using System;
using System.Collections.Immutable;
using Bud.Cli;
using Newtonsoft.Json;

namespace Bud.Commander {
  public static class CommandEvaluator {
    public static string EvaluateToJsonSync(string command, ref BuildContext buildContext) {
      try {
        return JsonConvert.SerializeObject(EvaluateSync(buildContext.Context, command));
      } catch (Exception ex) {
        throw new OperationCanceledException(string.Format("Task '{0}' failed.", command), ex);
      }
    }

    public static string EvaluateMacroToJsonSync(string macroName, string[] commandLineParameters, ref BuildContext buildContext) {
      ImmutableDictionary<string, Macro> macros;
      Macro macro;
      if (buildContext.Config.TryEvaluate(Macro.Macros, out macros)
          && macros.TryGetValue(macroName, out macro)) {
        var macroResult = macro.Function(buildContext, commandLineParameters);
        buildContext = macroResult.BuildContext;
        return JsonConvert.SerializeObject(macroResult.Value);
      }
      throw new ArgumentException(string.Format("Could not find macro '{0}'.", macroName));
    }

    private static object EvaluateSync(IContext context, string command) => context.EvaluateKeySync(Key.Parse(command));
  }
}