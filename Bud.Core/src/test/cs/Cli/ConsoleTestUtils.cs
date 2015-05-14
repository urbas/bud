using System;

namespace Bud.Cli {
  internal static class ConsoleTestUtils {
    public static ConsoleKeyInfo ToConsoleKeyInfo(char chr) {
      return new ConsoleKeyInfo(chr, ToConsoleKey(chr), false, false, false);
    }

    public static ConsoleKey ToConsoleKey(char chr) {
      return chr == '.' ? ConsoleKey.OemPeriod : (ConsoleKey) Char.ToUpperInvariant(chr);
    }

    public static ConsoleKeyInfo ToConsoleKeyInfo(ConsoleKey consoleKey) {
      return ToConsoleKeyInfo(consoleKey, 0);
    }

    public static ConsoleKeyInfo ToConsoleKeyInfo(ConsoleKey consoleKey, ConsoleModifiers consoleModifiers) {
      return new ConsoleKeyInfo((char) consoleKey,
                                consoleKey,
                                consoleModifiers.HasFlag(ConsoleModifiers.Shift),
                                consoleModifiers.HasFlag(ConsoleModifiers.Alt),
                                consoleModifiers.HasFlag(ConsoleModifiers.Control));
    }
  }
}