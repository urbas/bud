using System;
using System.Collections.Generic;
using System.Collections.Immutable;

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
        while (TryExtractCommand(argumentsEnumerator, commandList)) {}
      }
      return commandList;
    }

    private static bool TryExtractCommand(IEnumerator<string> commandLineArguments, ICollection<Command> commands) {
      if (IsMacroCommandSeparator(commandLineArguments.Current)) {
        return commandLineArguments.MoveNext();
      }
      if (MacroCommand.IsMacroCommand(commandLineArguments.Current)) {
        bool hasMoreArguments;
        commands.Add(FromCommandLineArguments(commandLineArguments, out hasMoreArguments));
        return hasMoreArguments;
      }
      commands.Add(new KeyCommand(commandLineArguments.Current));
      return commandLineArguments.MoveNext();
    }

    private static bool IsMacroCommandSeparator(string commandLineArgument) => Macro.MacroNamePrefix.Equals(commandLineArgument);

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

    public static void ExtractOptionsAndCommands(string[] commandLineArgs, out string[] options, out string[] commands) {
      int indexOfSeparator = Array.FindIndex(commandLineArgs, MacroCommand.IsMacroCommand);
      if (indexOfSeparator < 0) {
        options = commandLineArgs;
        commands = new string[] {};
      } else {
        options = new string[indexOfSeparator];
        Array.Copy(commandLineArgs, options, options.Length);
        commands = new string[commandLineArgs.Length - indexOfSeparator];
        Array.Copy(commandLineArgs, indexOfSeparator, commands, 0, commands.Length);
      }
    }
  }
}