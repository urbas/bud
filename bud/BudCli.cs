using System.IO;
using Bud.Commander;

namespace Bud {
  public static class BudCli {
    public static void Main(string[] args) {
      var buildCommander = BuildCommander.Load(Directory.GetCurrentDirectory());
      buildCommander.Evaluate(GetCommandToExecute(args));
    }

    private static string GetCommandToExecute(string[] args) {
      var commandToExecute = "build";
      if (args.Length > 0) {
        commandToExecute = args[0];
      }
      return commandToExecute;
    }
  }
}