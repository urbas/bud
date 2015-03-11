using System;
using System.Collections.Generic;
using System.IO;
using Bud.Commander;

namespace Bud {
  public static class BudCli {
    private static readonly string[] DefaultCommandsToExecute = {"build"};

    public static void Main(string[] args) {
      var cliArguments = new CliArguments();
      if (CommandLine.Parser.Default.ParseArguments(args, cliArguments)) {
        var buildCommander = cliArguments.BuildLevel ? BuildCommander.LoadBuildLevelCommander(Directory.GetCurrentDirectory()) : BuildCommander.Load(Directory.GetCurrentDirectory());
        ExecuteCommands(CommandsToExecute(cliArguments), buildCommander);
      } else {
        Console.Error.Write(cliArguments.GetUsage());
      }
    }

    private static IEnumerable<string> CommandsToExecute(CliArguments cliArguments) {
      return cliArguments.Commands.Count == 0 ? DefaultCommandsToExecute : cliArguments.Commands;
    }

    private static void ExecuteCommands(IEnumerable<string> commandsToExecute, IBuildCommander buildCommander) {
      foreach (var command in commandsToExecute) {
        try {
          buildCommander.Evaluate(command);
        } catch (Exception e) {
          Console.Error.WriteLine("An error occurred during the execution of the command '{0}'. Error messages:", command);
          PrintItemizedErrorMessages(new[] {e}, 0);
          break;
        }
      }
    }

    private static void PrintItemizedErrorMessages(IEnumerable<Exception> exceptions, int depth) {
      foreach (var exception in exceptions) {
        var aggregateException = exception as AggregateException;
        if (aggregateException != null) {
          PrintItemizedErrorMessages(aggregateException.InnerExceptions, depth);
        } else if (exception.InnerException != null) {
          PrintErrorMessageItem(depth, exception);
          PrintItemizedErrorMessages(new[] {exception.InnerException}, depth + 1);
        } else {
          PrintErrorMessageItem(depth, exception);
        }
      }
    }

    private static void PrintErrorMessageItem(int depth, Exception exception) {
      for (int i = 0; i <= depth; i++) {
        Console.Error.Write(" ");
      }
      Console.Error.Write("- ");
      Console.Error.WriteLine(exception.Message);
    }
  }
}