using System;
using Newtonsoft.Json;

namespace Bud.Commander {
  public static class CommandEvaluator {
    public static string EvaluateToJsonSync(string command, ref BuildCommanderContext buildCommanderContext) {
      if (IsMacroCommand(command)) {
        return EvaluateMacroSync(command, ref buildCommanderContext);
      }
      return EvaluateToJsonSync(buildCommanderContext.Context, command);
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

    private static string EvaluateMacroSync(string command, ref BuildCommanderContext buildCommanderContext) {
      var macros = buildCommanderContext.Config.Evaluate(Macro.Macros);
      var macro = macros[command.Substring(1)];
      var newSettings = macro.Function(buildCommanderContext.Settings, new string[] {});
      buildCommanderContext = buildCommanderContext.UpdateSettings(newSettings);
      return null;
    }
  }
}