using System;
using Newtonsoft.Json;

namespace Bud.Commander {
  public static class CommandEvaluator {
    public static string EvaluateToJsonSync(string command, ref BuildContext buildContext) {
      if (IsMacroCommand(command)) {
        return EvaluateMacroSync(command, ref buildContext);
      }
      return EvaluateToJsonSync(buildContext.Context, command);
    }

    private static bool IsMacroCommand(string command) => command.StartsWith(Macro.MacroNamePrefix);

    private static string EvaluateToJsonSync(IContext context, string command) {
      try {
        return JsonConvert.SerializeObject(EvaluateSync(context, command));
      } catch (Exception ex) {
        throw new OperationCanceledException(String.Format("Task '{0}' failed.", command), ex);
      }
    }

    private static object EvaluateSync(IContext context, string command) => context.EvaluateKeySync(Key.Parse(command));

    private static string EvaluateMacroSync(string command, ref BuildContext buildContext) {
      var macros = buildContext.Config.Evaluate(Macro.Macros);
      var macro = macros[command.Substring(1)];
      var macroResult = macro.Function(buildContext, new string[] {});
      buildContext = macroResult.BuildContext;
      return JsonConvert.SerializeObject(macroResult.Value);
    }
  }
}