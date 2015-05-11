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

    public string Line => LineBuffer.ToString();

    public int LineLength => LineBuffer.Length;

    public int CursorPosition => (ConsoleBuffer.CursorTop - CursorStartTop + 1) * ConsoleBuffer.BufferWidth - CursorStartLeft - (ConsoleBuffer.BufferWidth - ConsoleBuffer.CursorLeft);

    private int CursorStartLeft { get; }

    private int CursorStartTop { get; }

    public void ProcessInput(ConsoleKeyInfo consoleKeyInfo) {
      switch (consoleKeyInfo.Key) {
        case ConsoleKey.Backspace:
          DeleteCharacterBeforeCursor();
          break;
        case ConsoleKey.Delete:
          DeleteCharacterAtCursor();
          break;
        case ConsoleKey.LeftArrow:
          MoveCursorLeft();
          break;
        case ConsoleKey.RightArrow:
          MoveCursorRight();
          break;
        case ConsoleKey.Home:
          MoveCursorToStart();
          break;
        case ConsoleKey.End:
          MoveCursorToEnd();
          break;
        default:
          InsertCharacterAtCursor(consoleKeyInfo.KeyChar);
          break;
      }
    }

    private void InsertCharacterAtCursor(char character) {
      ConsoleBuffer.ShiftBufferRight(ConsoleBuffer.CursorLeft,
                                     ConsoleBuffer.CursorTop,
                                     LineLength - CursorPosition);
      LineBuffer.Insert(CursorPosition, character);
      ConsoleBuffer.Write(character);
    }

    private void DeleteCharacterBeforeCursor() {
      if (CursorPosition == 0) {
        return;
      }
      LineBuffer.Remove(CursorPosition - 1, 1);
      MoveCursorLeft();
      ConsoleBuffer.ShiftBufferLeft(ConsoleBuffer.CursorLeft + 1,
                                    ConsoleBuffer.CursorTop,
                                    LineLength - CursorPosition + 1);
    }

    private void DeleteCharacterAtCursor() {
      if (CursorPosition >= LineLength) {
        return;
      }
      LineBuffer.Remove(CursorPosition, 1);
      ConsoleBuffer.ShiftBufferLeft(ConsoleBuffer.CursorLeft + 1,
                                    ConsoleBuffer.CursorTop,
                                    LineLength - CursorPosition + 1);
    }

    private void MoveCursorLeft() {
      if (CursorPosition == 0) {
        return;
      }
      ConsoleBuffer.DecrementCursorPosition();
    }

    private void MoveCursorRight() {
      if (CursorPosition >= LineLength) {
        return;
      }
      ConsoleBuffer.IncrementCursorPosition();
    }

    private void MoveCursorToStart() {
      ConsoleBuffer.CursorLeft = CursorStartLeft;
      ConsoleBuffer.CursorTop = CursorStartTop;
    }

    private void MoveCursorToEnd() {
      ConsoleBuffer.CursorLeft = (CursorStartLeft + LineLength) % ConsoleBuffer.BufferWidth;
      ConsoleBuffer.CursorTop = CursorStartTop + (CursorStartLeft + LineLength) / ConsoleBuffer.BufferWidth;
    }
  }
}