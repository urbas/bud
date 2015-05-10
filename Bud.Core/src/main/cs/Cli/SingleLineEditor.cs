using System;
using System.Text;

namespace Bud.Cli {
  public class SingleLineEditor {
    private readonly IConsoleBuffer ConsoleBuffer;
    private readonly StringBuilder LineBuffer = new StringBuilder();

    public SingleLineEditor(IConsoleBuffer consoleBuffer) {
      ConsoleBuffer = consoleBuffer;
      CursorStartLeft = consoleBuffer.CursorLeft;
      CursorStartTop = consoleBuffer.CursorTop;
    }

    public static SingleLineEditor Create() => new SingleLineEditor(new ConsoleBuffer());

    public string Line => LineBuffer.ToString();

    public int LineLength => LineBuffer.Length;

    public int CursorPosition => (ConsoleBuffer.CursorTop - CursorStartTop + 1) * ConsoleBuffer.BufferWidth - CursorStartLeft - (ConsoleBuffer.BufferWidth - ConsoleBuffer.CursorLeft);

    private int CursorStartLeft { get; }

    private int CursorStartTop { get; }

    private bool IsLineEmpty => LineLength == 0;

    private bool IsCursorInsideLine => ConsoleBuffer.CursorLeft < LineLength;

    public void ProcessInput(ConsoleKeyInfo consoleKeyInfo) {
      if (consoleKeyInfo.Key == ConsoleKey.Backspace) {
        DeleteCurrentCharacter();
      } else if (IsKeyInRange(consoleKeyInfo.Key, ConsoleKey.PageUp, ConsoleKey.DownArrow)) {
        MoveCursor(consoleKeyInfo);
      } else {
        AppendCharacter(consoleKeyInfo.KeyChar);
      }
    }

    private void AppendCharacter(char character) {
      if (IsCursorInsideLine) {
        PushCharactersAfterCursor();
      }
      LineBuffer.Insert(CursorPosition, character);
      ConsoleBuffer.Write(character);
    }

    private void PushCharactersAfterCursor() {
      var lengthAfterCursor = LineLength - CursorPosition;
      var lastRow = ConsoleBuffer.CursorTop + (ConsoleBuffer.CursorLeft + lengthAfterCursor) / ConsoleBuffer.BufferWidth;
      var lastColumn = (ConsoleBuffer.CursorLeft + lengthAfterCursor) % ConsoleBuffer.BufferWidth;
      if (lastRow == ConsoleBuffer.CursorTop) {
        PushBufferLineByOne(lastRow, ConsoleBuffer.CursorLeft, lengthAfterCursor);
      }
      // TODO: Transform the buffer also in the case when the region to push spans multiple lines
    }

    private void PushBufferLineByOne(int row, int startColumn, int length) {
      ConsoleBuffer.MoveArea(startColumn, row, length, 1, startColumn + 1, row);
    }

    private void DeleteCurrentCharacter() {
      if (IsLineEmpty) {
        return;
      }
      LineBuffer.Remove(ConsoleBuffer.CursorLeft - 1, 1);
      MoveCursorBackwards();
      ConsoleBuffer.MoveArea(ConsoleBuffer.CursorLeft + 1,
                             ConsoleBuffer.CursorTop,
                             LineLength - ConsoleBuffer.CursorLeft + 1,
                             1,
                             ConsoleBuffer.CursorLeft,
                             ConsoleBuffer.CursorTop);
    }

    private void MoveCursor(ConsoleKeyInfo consoleKeyInfo) {
      switch (consoleKeyInfo.Key) {
        case ConsoleKey.LeftArrow:
          MoveCursorBackwards();
          break;
        case ConsoleKey.RightArrow:
          MoveCursorForwards();
          break;
        case ConsoleKey.Home:
          ConsoleBuffer.CursorLeft = 0;
          break;
        case ConsoleKey.End:
          ConsoleBuffer.CursorLeft = LineLength;
          break;
      }
    }

    private void MoveCursorBackwards() {
      if (ConsoleBuffer.CursorLeft <= CursorStartLeft) {
        return;
      }
      --ConsoleBuffer.CursorLeft;
    }

    private void MoveCursorForwards() {
      if (ConsoleBuffer.CursorLeft >= LineLength) {
        return;
      }
      ++ConsoleBuffer.CursorLeft;
    }

    private static bool IsKeyInRange(ConsoleKey key,
                                     ConsoleKey lowerBound,
                                     ConsoleKey upperBound)
      => lowerBound <= key && key <= upperBound;
  }
}