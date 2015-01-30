using System;

namespace Bud.Commander {
  public interface IBuildCommander : IDisposable {
    object Evaluate(string command);
  }

  public static class BuildCommanderExtensions {
    public static object Evaluate(this IBuildCommander buildCommander, Key command) {
      return buildCommander.Evaluate(command.ToString());
    }
  }
}