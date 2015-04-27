using System;
using System.Collections.Generic;
using System.IO;
using Bud.Build;
using Bud.Commander;
using Bud.Util;

namespace Bud {
  public static class BudCli {
    private static readonly string[] DefaultCommandsToExecute = {BuildKeys.Build.Id};

    public static void Main(string[] args) {
      var cliArguments = new CliArguments();
      if (CommandLine.Parser.Default.ParseArguments(args, cliArguments)) {
        try {
          InterpretArguments(cliArguments);
        } catch (Exception e) {
          ExceptionUtils.PrintItemizedErrorMessages(e);
          Environment.ExitCode = 1;
        }
      }
    }

    private static void InterpretArguments(CliArguments cliArguments) {
      if (cliArguments.IsShowVersion) {
        Console.WriteLine(BudVersion.Current);
        return;
      }
      ExecuteCommands(GetCommandsToExecute(cliArguments),
                      LoadBuildCommander(cliArguments),
                      cliArguments.PrintJson);
    }

    private static void ExecuteCommands(IEnumerable<string> commandsToExecute, IBuildCommander buildCommander, bool printJsonValue) {
      foreach (var command in commandsToExecute) {
        var valueAsJson = buildCommander.EvaluateToJson(command);
        if (printJsonValue) {
          Console.WriteLine(valueAsJson);
        }
      }
    }

    private static IEnumerable<string> GetCommandsToExecute(CliArguments cliArguments) {
      return cliArguments.Commands.Count == 0 ? DefaultCommandsToExecute : cliArguments.Commands;
    }

    private static IBuildCommander LoadBuildCommander(CliArguments cliArguments) {
      try {
        return BuildCommander.LoadBuildCommander(cliArguments.BuildLevel, cliArguments.IsQuiet, Directory.GetCurrentDirectory());
      } catch (Exception e) {
        throw new Exception("An error occurred during build initialiation.", e);
      }
    }
  }
}