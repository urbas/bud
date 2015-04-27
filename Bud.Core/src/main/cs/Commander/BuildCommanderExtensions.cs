using Newtonsoft.Json;

namespace Bud.Commander {
  public static class BuildCommanderExtensions {
    public static string EvaluateToJson(this IBuildCommander buildCommander, Key command) {
      return buildCommander.EvaluateToJson(command.ToString());
    }

    public static T Evaluate<T>(this IBuildCommander buildCommander, ConfigKey<T> command) => Evaluate<T>(buildCommander, command.ToString());

    public static T Evaluate<T>(this IBuildCommander buildCommander, TaskKey<T> command) => Evaluate<T>(buildCommander, command.ToString());

    public static T Evaluate<T>(this IBuildCommander buildCommander, string commandString) {
      return JsonConvert.DeserializeObject<T>(buildCommander.EvaluateToJson(commandString));
    }

    public static T EvaluateMacro<T>(this IBuildCommander buildCommander, string macroName, params string[] parameters) {
      return JsonConvert.DeserializeObject<T>(buildCommander.EvaluateMacroToJson(macroName, parameters));
    }
  }
}