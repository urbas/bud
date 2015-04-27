using Bud.Commander;

namespace Bud.Cli {
  public abstract class Command {
    public abstract string EvaluateToJson(IBuildCommander buildCommander);
  }
}