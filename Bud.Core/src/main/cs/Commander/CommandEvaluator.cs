using System;
using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Bud.Commander {
  internal class CommandEvaluator : ICommandEvaluator {
    public string EvaluateToJsonSync(string command, ref IBuildContext context) {
      try {
        return JsonConvert.SerializeObject(EvaluateSync(context.Context, command));
      } catch (Exception ex) {
        throw new OperationCanceledException($"Task '{command}' failed.", ex);
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
      throw new ArgumentException($"Could not find macro '{macroName}'.");
    }

    public static object EvaluateSync(IContext context, string command) => context.EvaluateKeySync(Key.Parse(command));
  }
}