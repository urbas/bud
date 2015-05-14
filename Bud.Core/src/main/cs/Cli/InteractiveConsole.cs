using System;
using System.Collections.Generic;
using System.Linq;
using Bud.Commander;
using Bud.Util;

namespace Bud.Cli {
  public class InteractiveConsole : IBuildCommander {
    private IBuildContext Context;
    private readonly IConsoleInput ConsoleInput;
    private readonly IConsoleBuffer ConsoleBuffer;
    private readonly ICommandEvaluator CommandEvaluator;
    private readonly ConsoleHistory ConsoleHistory = new ConsoleHistory();
    public LineEditor Editor { get; private set; }

    public InteractiveConsole(IBuildContext context, IConsoleInput consoleInput, IConsoleBuffer consoleBuffer, ICommandEvaluator commandEvaluator) {
      Context = context;
      ConsoleInput = consoleInput;
      ConsoleBuffer = consoleBuffer;
      CommandEvaluator = commandEvaluator;
    }

    public void DisplayInstructions() {
      ConsoleBuffer.WriteLine(string.Format("Press '{0} + {1}' to quit the interactive console.", ConsoleModifiers.Control, ConsoleKey.Q));
    }

    public void Serve() {
      StartNewInputLine();
      ConsoleHistory.AddHistoryEntry();
      while (ActOnUserInput()) {}
    }

    public void Dispose() {}

    public string EvaluateToJson(string command)
      => CommandEvaluator.EvaluateToJsonSync(command, ref Context);

    public string EvaluateMacroToJson(string macroName, params string[] commandLineParameters)
      => CommandEvaluator.EvaluateMacroToJsonSync(macroName, commandLineParameters, ref Context);

    private bool ActOnUserInput() {
      var consoleKeyInfo = ConsoleInput.ReadKey();
      if (IsExitKeyCombination(consoleKeyInfo)) {
        return false;
      }
      switch (consoleKeyInfo.Key) {
        case ConsoleKey.Enter:
          ConsoleHistory.AddHistoryEntry();
          EvaluateCommands();
          break;
        case ConsoleKey.Tab:
          SuggestCompletions();
          ConsoleHistory.RecordHistoryEntry(Editor);
          break;
        case ConsoleKey.UpArrow:
          ConsoleHistory.ShowPreviousHistoryEntry(Editor);
          ConsoleHistory.RecordHistoryEntry(Editor);
          break;
        case ConsoleKey.DownArrow:
          ConsoleHistory.ShowNextHistoryEntry(Editor);
          break;
        default:
          Editor.ProcessInput(consoleKeyInfo);
          ConsoleHistory.RecordHistoryEntry(Editor);
          break;
      }
      return true;
    }

    private void EvaluateCommands() {
      var commands = CommandListParser.ParseCommandLine(Editor.Line);
      foreach (var command in commands) {
        EvaluateCommand(command);
      }
      StartNewInputLine();
    }

    private void EvaluateCommand(Command command) {
      ConsoleBuffer.WriteLine();
      var evaluationResult = EvaluateInputToJson(command);
      if (evaluationResult != null) {
        ConsoleBuffer.WriteLine(command.Name + " = " + evaluationResult);
      }
    }

    private void SuggestCompletions() {
      ConsoleBuffer.WriteLine();
      var partialCommand = Editor.Line.Substring(0, Editor.CursorPosition);
      var suggestions = GetSuggestions(partialCommand);
      PrintSuggestions(suggestions);
      RedrawInputLine();
      ApplyCompletion(suggestions, partialCommand);
    }

    private void ApplyCompletion(IEnumerable<string> suggestions, string partialCommand) {
      var completionString = GetCompletionString(suggestions, partialCommand);
      if (Editor.LineLength < completionString.Length) {
        Editor.InsertText(completionString.Substring(Editor.Line.Length));
      }
    }

    private static string GetCompletionString(IEnumerable<string> suggestions, string partialCommand) {
      var commonPrefix = StringUtils.CommonPrefix(suggestions);
      if (KeyPathUtils.StartsWithKeySeparator(partialCommand)) {
        return commonPrefix;
      }
      return KeyPathUtils.RemoveKeySeparatorPrefix(commonPrefix);
    }

    private static void PrintSuggestions(IEnumerable<string> suggestions) {
      foreach (var suggestion in suggestions) {
        Console.WriteLine(suggestion);
      }
    }

    private IEnumerable<string> GetSuggestions(string partialCommand) {
      var absoluteKey = KeyPathUtils.PrependKeySeparator(partialCommand);
      IEnumerable<string> configs = Context.Settings.ConfigDefinitions.Keys.Select(key => key.Path);
      IEnumerable<string> tasks = Context.Settings.TaskDefinitions.Keys.Select(key => key.Path);
      IEnumerable<string> macroNames = Context.Config.Evaluate(Macro.Macros).Values.Select(macro => Macro.MacroNamePrefix + macro.Name);
      return configs.Concat(tasks)
                    .Concat(macroNames)
                    .Where(suggestion => suggestion.StartsWith(partialCommand) || suggestion.StartsWith(absoluteKey))
                    .OrderBy(key => key);
    }

    private string EvaluateInputToJson(Command command) {
      try {
        var evaluationResult = command.EvaluateToJson(this);
        return "null".Equals(evaluationResult) ? null : evaluationResult;
      } catch (Exception e) {
        ExceptionUtils.PrintItemizedErrorMessages(e, true);
        return null;
      }
    }

    private void StartNewInputLine() {
      PrintPrompt();
      Editor = new LineEditor(ConsoleBuffer);
    }

    private void RedrawInputLine() {
      PrintPrompt();
      Editor.ResetOrigin();
    }

    private void PrintPrompt() => ConsoleBuffer.Write("bud> ");

    private static bool IsExitKeyCombination(ConsoleKeyInfo consoleKeyInfo)
      => consoleKeyInfo.Key == ConsoleKey.Q &&
         consoleKeyInfo.Modifiers == ConsoleModifiers.Control;
  }
}