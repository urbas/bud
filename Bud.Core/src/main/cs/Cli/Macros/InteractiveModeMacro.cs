using System;

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
    private readonly BuildContext Context;
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
      if (consoleKeyInfo.Key == ConsoleKey.Enter) {
        Console.WriteLine();
        Console.WriteLine(SingleLineEditor.Line);
        StartNewInputLine();
      } else {
        SingleLineEditor.ProcessInput(consoleKeyInfo);
      }

      return true;
    }

    private void StartNewInputLine() {
      PrintPromptPrefix();
      SingleLineEditor = SingleLineEditor.Create();
    }

    private static void PrintPromptPrefix() {
      Console.Write("bud> ");
    }

    private static bool IsExitKeyCombination(ConsoleKeyInfo consoleKeyInfo) {
      return consoleKeyInfo.Key == ConsoleKey.Q &&
             consoleKeyInfo.Modifiers == ConsoleModifiers.Control;
    }
  }
}