using System;
using Bud.Commander;
using Bud.Util;

namespace Bud.Cli.Macros {
  public static class InteractiveModeMacro {
    public static MacroResult InteractiveModeMacroImpl(BuildContext context, string[] commandlinearguments) {
      var interactiveConsole = new InteractiveConsole(context);
      interactiveConsole.DisplayInstructions();
      interactiveConsole.Serve();
      return new MacroResult(null, context);
    }
  }

  public class InteractiveConsole {
    private BuildContext Context;
    private SingleLineEditor SingleLineEditor;

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

    public bool ActOnUserInput() {
      var consoleKeyInfo = Console.ReadKey(true);
      if (IsExitKeyCombination(consoleKeyInfo)) {
        return false;
      }
      switch (consoleKeyInfo.Key) {
        case ConsoleKey.Enter:
          EvaluateLine();
          break;
        default:
          SingleLineEditor.ProcessInput(consoleKeyInfo);
          break;
      }

      return true;
    }

    private void EvaluateLine() {
      Console.WriteLine();
      var evaluationResult = EvaluateInputToJson();
      Console.WriteLine(SingleLineEditor.Line + " = " + evaluationResult);
      StartNewInputLine();
    }

    private string EvaluateInputToJson() {
      try {
        var evaluationResult = CommandEvaluator.EvaluateToJsonSync(SingleLineEditor.Line, ref Context);
        return "null".Equals(evaluationResult) ? null : evaluationResult;
      } catch (Exception e) {
        ExceptionUtils.PrintItemizedErrorMessages(e, true);
        return null;
      }
    }

    private void StartNewInputLine() {
      PrintPrompt();
      SingleLineEditor = new SingleLineEditor(new ConsoleBuffer());
    }

    private static void PrintPrompt() => Console.Write("bud> ");

    private static bool IsExitKeyCombination(ConsoleKeyInfo consoleKeyInfo) {
      return consoleKeyInfo.Key == ConsoleKey.Q &&
             consoleKeyInfo.Modifiers == ConsoleModifiers.Control;
    }
  }
}