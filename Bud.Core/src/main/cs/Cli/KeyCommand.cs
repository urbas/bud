using Bud.Commander;

namespace Bud.Cli {
  public class KeyCommand : Command {
    public string Key { get; }

    public KeyCommand(string key) {
      Key = key;
    }

    public override string EvaluateToJson(IBuildCommander buildCommander) => buildCommander.EvaluateToJson(Key);
  }
}