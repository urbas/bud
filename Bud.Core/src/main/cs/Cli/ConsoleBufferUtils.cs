using System;

namespace Bud.Cli {
  public static class ConsoleBufferUtils {
    public static void ShiftBufferRight(this IConsoleBuffer consoleBuffer, int startColumn, int startRow, int numberOfCharsToShift) {
      int currentRow = startRow + (startColumn + numberOfCharsToShift - 1) / consoleBuffer.BufferWidth;
      if (currentRow != startRow) {
        ShiftLastRowRight(consoleBuffer, startColumn, ref numberOfCharsToShift, ref currentRow);
        while (currentRow != startRow) {
          ShiftSingleRowRight(consoleBuffer, ref numberOfCharsToShift, ref currentRow, consoleBuffer.BufferWidth);
        }
      }
      ShiftSingleRowRight(consoleBuffer, startColumn, startRow, numberOfCharsToShift);
    }

    public static void ShiftBufferLeft(this IConsoleBuffer consoleBuffer, int startColumn, int startRow, int numberOfCharsToShift) {
      ShiftFirstRowLeft(consoleBuffer, startColumn, ref startRow, ref numberOfCharsToShift);
      while (numberOfCharsToShift > 0) {
        ShiftSingleRowLeft(consoleBuffer, 0, ref startRow, ref numberOfCharsToShift, Math.Min(consoleBuffer.BufferWidth, numberOfCharsToShift));
      }
    }

    private static void ShiftSingleRowRight(IConsoleBuffer consoleBuffer, int startColumn, int startRow, int numberOfCharsToShift) {
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
      ShiftSingleRowRight(consoleBuffer, 0, currentRow, lastRowCharCount);
      numberOfCharsToShift -= lastRowCharCount;
      --currentRow;
    }

    private static void ShiftLastRowRight(IConsoleBuffer consoleBuffer, int startColumn, ref int numberOfCharsToShift, ref int currentRow) {
      int charCountInLastRow = (startColumn + numberOfCharsToShift - 1) % consoleBuffer.BufferWidth + 1;
      ShiftSingleRowRight(consoleBuffer, ref numberOfCharsToShift, ref currentRow, charCountInLastRow);
    }

    private static void ShiftSingleRowLeft(IConsoleBuffer consoleBuffer, int startColumn, int startRow, int numberOfCharsToShift) {
      if (startColumn > 0) {
        consoleBuffer.MoveArea(startColumn, startRow, numberOfCharsToShift, 1, startColumn - 1, startRow);
      } else {
        consoleBuffer.MoveArea(0, startRow, 1, 1, consoleBuffer.BufferWidth - 1, startRow - 1);
        if (numberOfCharsToShift > 1) {
          consoleBuffer.MoveArea(1, startRow, numberOfCharsToShift - 1, 1, 0, startRow);
        }
      }
    }

    private static void ShiftFirstRowLeft(IConsoleBuffer consoleBuffer, int startColumn, ref int currentRow, ref int numberOfCharsToShift) {
      var charsInFirstRow = Math.Min(consoleBuffer.BufferWidth - startColumn, numberOfCharsToShift);
      ShiftSingleRowLeft(consoleBuffer, startColumn, ref currentRow, ref numberOfCharsToShift, charsInFirstRow);
    }

    private static void ShiftSingleRowLeft(IConsoleBuffer consoleBuffer, int startColumn, ref int currentRow, ref int numberOfCharsToShift, int charsToShift) {
      ShiftSingleRowLeft(consoleBuffer, startColumn, currentRow, charsToShift);
      numberOfCharsToShift -= charsToShift;
      ++currentRow;
    }
  }
}