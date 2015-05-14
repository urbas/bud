using System;

namespace Bud.Cli {
  public interface IConsoleInput {
    ConsoleKeyInfo ReadKey();
  }
}