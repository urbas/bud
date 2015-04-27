using System.Collections.Generic;

namespace Bud.Cli {
  public class MacroCommand : Command {
    public string MacroName { get; }
    public string[] Parameters { get; }

    public MacroCommand(string macroName, params string[] parameters) {
      MacroName = macroName;
      Parameters = parameters;
    }

    public static bool IsMacroCommand(string command) => command.StartsWith(Macro.MacroNamePrefix);
  }
}