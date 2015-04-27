using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Bud.Build;
using Bud.Cli;
using Bud.Commander;
using Bud.Util;
using CommandLine;

namespace Bud {
  public static class BudCli {
    private static readonly ImmutableList<Command> DefaultCommands = ImmutableList.Create<Command>(new KeyCommand(BuildKeys.Build.Id));

    public static void Main(string[] args) {
      var cliArguments = new CliArguments();
      if (Parser.Default.ParseArguments(args, cliArguments)) {
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

    private static void ExecuteCommands(IEnumerable<Command> commandsToExecute, IBuildCommander buildCommander, bool printJsonValue) {
      foreach (var command in commandsToExecute) {
        var valueAsJson = command.EvaluateToJson(buildCommander);
        if (printJsonValue) {
          Console.WriteLine(valueAsJson);
        }
      }
    }

    private static IEnumerable<Command> GetCommandsToExecute(CliArguments cliArguments) {
      if (cliArguments.Commands.Count == 0) {
        return DefaultCommands;
      }
      return CommandListParser.ToCommandList(cliArguments.Commands);
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