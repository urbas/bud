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
      int currentRow = startRow + (startColumn + numberOfCharsToShift - 1) / consoleBuffer.BufferWidth;
      if (currentRow != startRow) {
        int charCountInLastRow = (startColumn + numberOfCharsToShift - 1) % consoleBuffer.BufferWidth + 1;
        ShiftSingleRowRight(consoleBuffer, ref numberOfCharsToShift, ref currentRow, charCountInLastRow);
        while (currentRow != startRow) {
          ShiftSingleRowRight(consoleBuffer, ref numberOfCharsToShift, ref currentRow, consoleBuffer.BufferWidth);
        }
      }
      ShiftSingleRowRight(consoleBuffer, startRow, startColumn, numberOfCharsToShift);
    }

    public static void ShiftBufferLeft(this IConsoleBuffer consoleBuffer, int startRow, int startColumn, int numberOfCharsToShift) {
      consoleBuffer.MoveArea(startColumn, startRow, numberOfCharsToShift, 1, startColumn - 1, startRow);
    }

    private static void ShiftSingleRowRight(IConsoleBuffer consoleBuffer, int startRow, int startColumn, int numberOfCharsToShift) {
      if (startColumn + numberOfCharsToShift < consoleBuffer.BufferWidth) {
        consoleBuffer.MoveArea(startColumn, startRow, numberOfCharsToShift, 1, startColumn + 1, startRow);
      } else {
        consoleBuffer.MoveArea(startColumn + numberOfCharsToShift - 1, startRow, 1, 1, 0, startRow + 1);
        if (numberOfCharsToShift > 1) {
          consoleBuffer.MoveArea(startColumn, startRow, numberOfCharsToShift - 1, 1, startColumn + 1, startRow);
        }
      }
    }

    private static void ShiftSingleRowRight(IConsoleBuffer consoleBuffer, ref int numberOfCharsToShift, ref int currentRow, int lastRowCharCount) {
      ShiftSingleRowRight(consoleBuffer, currentRow, 0, lastRowCharCount);
      numberOfCharsToShift -= lastRowCharCount;
      --currentRow;
    }
  }
}