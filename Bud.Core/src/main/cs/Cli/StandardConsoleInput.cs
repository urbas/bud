using System;

namespace Bud.Cli {
  public class StandardConsoleInput : IConsoleInput {
    public ConsoleKeyInfo ReadKey() {
      return Console.ReadKey(true);
    }
  }
}