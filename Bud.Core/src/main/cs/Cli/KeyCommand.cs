using Bud.Commander;

namespace Bud.Cli {
  public class KeyCommand : Command {
    public override string Name { get; }

    public KeyCommand(string name) {
      Name = name;
    }

    public override string EvaluateToJson(IBuildCommander buildCommander) => buildCommander.EvaluateToJson(Name);
  }
}