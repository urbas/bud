using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bud.Cli {
  public static class CommandListParser {
    public static IEnumerable<Command> ToCommandList(IEnumerable<string> commandLineArguments) {
      if (commandLineArguments == null) {
        return ImmutableList<Command>.Empty;
      }
      return ExtractCommands(commandLineArguments.GetEnumerator());
    }

    private static IEnumerable<Command> ExtractCommands(IEnumerator<string> argumentsEnumerator) {
      var commandList = new List<Command>();
      if (argumentsEnumerator.MoveNext()) {
        while (true) {
          bool hasMoreArguments;
          commandList.Add(ExtractCommand(argumentsEnumerator, out hasMoreArguments));
          if (!hasMoreArguments) {
            break;
          }
        }
      }
      return commandList;
    }

    private static Command ExtractCommand(IEnumerator<string> argumentsEnumerator, out bool hasMoreArguments) {
      if (MacroCommand.IsMacroCommand(argumentsEnumerator.Current)) {
        return FromCommandLineArguments(argumentsEnumerator, out hasMoreArguments);
      }
      var keyCommand = new KeyCommand(argumentsEnumerator.Current);
      hasMoreArguments = argumentsEnumerator.MoveNext();
      return keyCommand;
    }

    private static MacroCommand FromCommandLineArguments(IEnumerator<string> argumentsEnumerator, out bool hasMoreArguments) {
      return new MacroCommand(argumentsEnumerator.Current.Substring(1), ExtractParameters(argumentsEnumerator, out hasMoreArguments));
    }

    private static string[] ExtractParameters(IEnumerator<string> argumentsEnumerator, out bool hasMoreArguments) {
      var macroParameters = new List<string>();
      while (argumentsEnumerator.MoveNext()) {
        if (MacroCommand.IsMacroCommand(argumentsEnumerator.Current)) {
          hasMoreArguments = true;
          return macroParameters.ToArray();
        }
        macroParameters.Add(argumentsEnumerator.Current);
      }
      hasMoreArguments = false;
      return macroParameters.ToArray();
    }
  }
}