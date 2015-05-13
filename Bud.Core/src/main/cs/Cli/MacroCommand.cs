using System.Collections.Generic;
using Bud.Commander;

namespace Bud.Cli {
  public class MacroCommand : Command {
    public override string Name { get; }

    public string[] Parameters { get; }

    public MacroCommand(string name, params string[] parameters) {
      Name = name;
      Parameters = parameters;
    }

    public override string EvaluateToJson(IBuildCommander buildCommander) {
      return buildCommander.EvaluateMacroToJson(Name, Parameters);
    }

    public static bool IsMacroCommand(string command) => command.StartsWith(Macro.MacroNamePrefix);
  }
}