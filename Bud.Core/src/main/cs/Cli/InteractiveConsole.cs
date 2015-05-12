using System;
using System.Collections.Generic;
using System.Linq;
using Bud.Commander;
using Bud.Util;

namespace Bud.Cli {
  public class InteractiveConsole {
    private BuildContext Context;
    private SingleLineEditor CurrentInput;

    public InteractiveConsole(BuildContext context) {
      Context = context;
    }

    public void DisplayInstructions() {
      Console.WriteLine(string.Format("Press '{0} + {1}' to quit the interactive console.", ConsoleModifiers.Control, ConsoleKey.Q));
    }

    public void Serve() {
      StartNewInputLine();
      while (ActOnUserInput()) {}
    }

    private bool ActOnUserInput() {
      var consoleKeyInfo = Console.ReadKey(true);
      if (IsExitKeyCombination(consoleKeyInfo)) {
        return false;
      }
      switch (consoleKeyInfo.Key) {
        case ConsoleKey.Enter:
          EvaluateLine();
          break;
        case ConsoleKey.Tab:
          SuggestCompletions();
          break;
        default:
          CurrentInput.ProcessInput(consoleKeyInfo);
          break;
      }

      return true;
    }

    private void EvaluateLine() {
      Console.WriteLine();
      var evaluationResult = EvaluateInputToJson();
      if (evaluationResult != null) {
        Console.WriteLine(CurrentInput.Line + " = " + evaluationResult);
      }
      StartNewInputLine();
    }

    private void SuggestCompletions() {
      Console.WriteLine();
      var partialCommand = CurrentInput.Line.Substring(0, CurrentInput.CursorPosition);
      var suggestions = GetSuggestions(partialCommand);
      PrintSuggestions(suggestions);
      RedrawInputLine();
      ApplyCompletion(suggestions, partialCommand);
    }

    private void ApplyCompletion(IEnumerable<string> suggestions, string partialCommand) {
      var completionString = GetCompletionString(suggestions, partialCommand);
      if (CurrentInput.LineLength < completionString.Length) {
        CurrentInput.InsertText(completionString.Substring(CurrentInput.Line.Length));
      }
    }

    private static string GetCompletionString(IEnumerable<string> suggestions, string partialCommand) {
      var commonPrefix = StringUtils.CommonPrefix(suggestions);
      if (!KeyPathUtils.StartsWithKeySeparator(partialCommand)) {
        commonPrefix = KeyPathUtils.RemoveKeySeparatorPrefix(commonPrefix);
      }
      return commonPrefix;
    }

    private static void PrintSuggestions(IEnumerable<string> suggestions) {
      foreach (var suggestion in suggestions) {
        Console.WriteLine(suggestion);
      }
    }

    private IEnumerable<string> GetSuggestions(string partialCommand) {
      partialCommand = KeyPathUtils.PrependKeySeparator(partialCommand);
      IEnumerable<IKey> configs = Context.ConfigDefinitions.Keys;
      IEnumerable<IKey> tasks = Context.TaskDefinitions.Keys;
      return configs.Concat(tasks)
                    .Select(key => key.Path)
                    .Where(key => key.StartsWith(partialCommand))
                    .OrderBy(key => key);
    }

    private string EvaluateInputToJson() {
      try {
        var evaluationResult = CommandEvaluator.EvaluateToJsonSync(CurrentInput.Line, ref Context);
        return "null".Equals(evaluationResult) ? null : evaluationResult;
      } catch (Exception e) {
        ExceptionUtils.PrintItemizedErrorMessages(e, true);
        return null;
      }
    }

    private void StartNewInputLine() {
      PrintPrompt();
      CurrentInput = new SingleLineEditor(new ConsoleBuffer());
    }

    private void RedrawInputLine() {
      PrintPrompt();
      CurrentInput.ResetOrigin();
    }

    private static void PrintPrompt() => Console.Write("bud> ");

    private static bool IsExitKeyCombination(ConsoleKeyInfo consoleKeyInfo)
      => consoleKeyInfo.Key == ConsoleKey.Q &&
         consoleKeyInfo.Modifiers == ConsoleModifiers.Control;
  }
}