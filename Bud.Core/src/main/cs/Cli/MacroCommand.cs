using System.Collections.Generic;
using Bud.Commander;

namespace Bud.Cli {
  public class MacroCommand : Command {
    public string MacroName { get; }
    public string[] Parameters { get; }

    public MacroCommand(string macroName, params string[] parameters) {
      MacroName = macroName;
      Parameters = parameters;
    }

    public override string EvaluateToJson(IBuildCommander buildCommander) {
      return buildCommander.EvaluateMacroToJson(MacroName, Parameters);
    }

    public static bool IsMacroCommand(string command) => command.StartsWith(Macro.MacroNamePrefix);
  }
}