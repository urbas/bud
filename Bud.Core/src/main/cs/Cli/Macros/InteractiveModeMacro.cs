using Bud.Commander;

namespace Bud.Cli.Macros {
  public static class InteractiveModeMacro {
    public const string InteractiveModeMacroName = "interactiveMode";

    public static MacroResult InteractiveModeMacroImpl(IBuildContext context, string[] commandlinearguments) {
      var interactiveConsole = new InteractiveConsole(context, new StandardConsoleInput(), new ConsoleBuffer(), new CommandEvaluator());
      interactiveConsole.DisplayInstructions();
      interactiveConsole.Serve();
      return new MacroResult(null, context);
    }
  }
}