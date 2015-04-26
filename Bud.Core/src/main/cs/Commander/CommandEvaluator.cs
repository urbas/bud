using System;
using System.Collections.Immutable;
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
        throw new OperationCanceledException(string.Format("Task '{0}' failed.", command), ex);
      }
    }

    private static object EvaluateSync(IContext context, string command) => context.EvaluateKeySync(Key.Parse(command));

    private static string EvaluateMacroSync(string command, ref BuildContext buildContext) {
      var macroName = command.Substring(1);
      ImmutableDictionary<string, Macro> macros;
      Macro macro;
      if (buildContext.Config.TryEvaluate(Macro.Macros, out macros)
          && macros.TryGetValue(macroName, out macro)) {
        var macroResult = macro.Function(buildContext, new string[] {});
        buildContext = macroResult.BuildContext;
        return JsonConvert.SerializeObject(macroResult.Value);
      }
      throw new ArgumentException(string.Format("Could not find macro '{0}'.", macroName));
    }
  }
}