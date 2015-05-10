namespace Bud.Cli {
  public static class ConsoleBufferUtils {
    public static void DecrementCursorPosition(this IConsoleBuffer consoleBuffer) {
      if (consoleBuffer.CursorLeft == 0) {
        --consoleBuffer.CursorTop;
        consoleBuffer.CursorLeft = consoleBuffer.BufferWidth - 1;
      } else {
        --consoleBuffer.CursorLeft;
      }
    }

    public static void IncrementCursorPosition(this IConsoleBuffer consoleBuffer) {
      if (consoleBuffer.CursorLeft >= consoleBuffer.BufferWidth - 1) {
        ++consoleBuffer.CursorTop;
        consoleBuffer.CursorLeft = 0;
      } else {
        ++consoleBuffer.CursorLeft;
      }
    }

    public static void ShiftBufferRight(this IConsoleBuffer consoleBuffer, int startRow, int startColumn, int numberOfCharsToShift) {
      consoleBuffer.MoveArea(startColumn, startRow, numberOfCharsToShift, 1, startColumn + 1, startRow);
    }

    public static void ShiftBufferLeft(this IConsoleBuffer consoleBuffer, int startRow, int startColumn, int numberOfCharsToShift) {
      consoleBuffer.MoveArea(startColumn, startRow, numberOfCharsToShift, 1, startColumn - 1, startRow);
    }
  }
}