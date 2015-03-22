using System;
using System.Collections.Generic;
using System.IO;
using Bud.Build;
using Bud.Commander;
using Bud.Commander.BuildCommander;
using Bud.Util;

namespace Bud {
  public static class BudCli {
    private static readonly string[] DefaultCommandsToExecute = {BuildKeys.Build.Id};

    public static void Main(string[] args) {
      var cliArguments = new CliArguments();
      if (CommandLine.Parser.Default.ParseArguments(args, cliArguments)) {
        ExecuteCommands(cliArguments);
      } else {
        Console.Error.Write(cliArguments.GetUsage());
      }
    }

    private static void ExecuteCommands(CliArguments cliArguments) {
      IBuildCommander buildCommander;
      try {
        buildCommander = LoadBuildCommander(cliArguments.BuildLevel, Directory.GetCurrentDirectory());
      } catch (Exception e) {
        Console.Error.WriteLine("An error occurred during build initialiation. Error messages:");
        ExceptionUtils.PrintItemizedErrorMessages(new[] {e}, 0);
        return;
      }
      var commandsToExecute = GetCommandsToExecute(cliArguments);
      ExecuteCommands(commandsToExecute, buildCommander);
    }

    private static IEnumerable<string> GetCommandsToExecute(CliArguments cliArguments) {
      return cliArguments.Commands.Count == 0 ? DefaultCommandsToExecute : cliArguments.Commands;
    }

    private static void ExecuteCommands(IEnumerable<string> commandsToExecute, IBuildCommander buildCommander) {
      foreach (var command in commandsToExecute) {
        try {
          buildCommander.Evaluate(command);
        } catch (Exception e) {
          Console.Error.WriteLine("An error occurred during the execution of command '{0}'. Error messages:", command);
          ExceptionUtils.PrintItemizedErrorMessages(new[] {e}, 0);
          break;
        }
      }
    }
  }
}