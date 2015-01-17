using System;
using System.IO;
using Bud.Commander;

namespace Bud {
  public static class BudCli {
    private static readonly string[] DefaultCommandsToExecute = {"build"};

    public static void Main(string[] args) {
      var buildCommander = BuildCommander.Load(Directory.GetCurrentDirectory());
      var commandsToExecute = args.Length == 0 ? DefaultCommandsToExecute : args;
      ExecuteCommands(commandsToExecute, buildCommander);
    }

    private static void ExecuteCommands(string[] commandsToExecute, IBuildCommander buildCommander) {
      foreach (var command in commandsToExecute) {
        buildCommander.Evaluate(command);
      }
    }
  }
}