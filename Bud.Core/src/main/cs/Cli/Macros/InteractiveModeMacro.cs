namespace Bud.Cli.Macros {
  public static class InteractiveModeMacro {
    public const string InteractiveModeMacroName = "interactiveMode";

    public static MacroResult InteractiveModeMacroImpl(BuildContext context, string[] commandlinearguments) {
      var interactiveConsole = new InteractiveConsole(context);
      interactiveConsole.DisplayInstructions();
      interactiveConsole.Serve();
      return new MacroResult(null, context);
    }
  }
}