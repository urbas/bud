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
        InterpretArguments(cliArguments);
      } else {
        Console.Error.Write(cliArguments.GetUsage());
      }
    }

    private static void InterpretArguments(CliArguments cliArguments) {
      if (cliArguments.IsShowVersion) {
        PrintVersion();
      } else if (!ExecuteCommands(cliArguments)) {
        Environment.ExitCode = 1;
      }
    }

    private static void PrintVersion() => Console.WriteLine(BudAssemblies.BudVersion);

    private static bool ExecuteCommands(CliArguments cliArguments) {
      IBuildCommander buildCommander;
      try {
        buildCommander = LoadBuildCommander(cliArguments.BuildLevel, cliArguments.IsQuiet, Directory.GetCurrentDirectory());
      } catch (Exception e) {
        Console.Error.WriteLine("An error occurred during build initialiation. Error messages:");
        ExceptionUtils.PrintItemizedErrorMessages(new[] {e}, 0);
        return false;
      }
      var commandsToExecute = GetCommandsToExecute(cliArguments);
      return ExecuteCommands(commandsToExecute, buildCommander, cliArguments.PrintJson);
    }

    private static IEnumerable<string> GetCommandsToExecute(CliArguments cliArguments) {
      return cliArguments.Commands.Count == 0 ? DefaultCommandsToExecute : cliArguments.Commands;
    }

    private static bool ExecuteCommands(IEnumerable<string> commandsToExecute, IBuildCommander buildCommander, bool printJsonValue) {
      foreach (var command in commandsToExecute) {
        try {
          var valueAsJson = buildCommander.EvaluateToJson(command);
          if (printJsonValue) {
            Console.WriteLine(valueAsJson);
          }
        } catch (Exception e) {
          Console.Error.WriteLine("An error occurred during the execution of command '{0}'. Error messages:", command);
          ExceptionUtils.PrintItemizedErrorMessages(new[] {e}, 0);
          return false;
        }
      }
      return true;
    }
  }
}