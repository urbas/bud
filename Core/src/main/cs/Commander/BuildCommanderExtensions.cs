namespace Bud.Commander {
  public static class BuildCommanderExtensions {
    public static object Evaluate(this IBuildCommander buildCommander, Key command) {
      return buildCommander.Evaluate(command.ToString());
    }
  }
}