using System;

namespace Bud.Cli.Macros {
  public static class InteractiveModeMacro {
    public static MacroResult InteractiveModeMacroImpl(BuildContext context, string[] commandlinearguments) {
      Console.WriteLine("Buffer size: {0}x{1}", Console.BufferWidth, Console.BufferHeight);
      Console.WriteLine("Window size: {0}x{1}", Console.WindowWidth, Console.WindowHeight);
      while (true) {
        var consoleKeyInfo = Console.ReadKey(true);
        Console.Write(consoleKeyInfo.KeyChar);
      }

      return new MacroResult(null, context);
    }
  }
}