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

    private bool IsCursorInsideLine => CursorPosition < LineLength;

    public void ProcessInput(ConsoleKeyInfo consoleKeyInfo) {
      switch (consoleKeyInfo.Key) {
        case ConsoleKey.Backspace:
          DeleteCurrentCharacter();
          break;
        case ConsoleKey.LeftArrow:
          MoveCursorLeft();
          break;
        case ConsoleKey.RightArrow:
          MoveCursorRight();
          break;
        case ConsoleKey.Home:
          ConsoleBuffer.CursorLeft = 0;
          break;
        case ConsoleKey.End:
          ConsoleBuffer.CursorLeft = LineLength;
          break;
        default:
          AppendCharacter(consoleKeyInfo.KeyChar);
          break;
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
        ShiftBufferRowRight(lastRow, ConsoleBuffer.CursorLeft, lengthAfterCursor);
      }
      // TODO: Transform the buffer also in the case when the region to push spans multiple lines
    }

    private void DeleteCurrentCharacter() {
      if (CursorPosition == 0) {
        return;
      }
      LineBuffer.Remove(CursorPosition - 1, 1);
      MoveCursorLeft();
      ShiftBufferRowLeft(ConsoleBuffer.CursorTop, ConsoleBuffer.CursorLeft + 1, LineLength - CursorPosition + 1);
    }

    private void ShiftBufferRowRight(int row, int startColumn, int length) {
      ConsoleBuffer.MoveArea(startColumn, row, length, 1, startColumn + 1, row);
    }

    private void ShiftBufferRowLeft(int row, int startColumn, int length) {
      ConsoleBuffer.MoveArea(startColumn, row, length, 1, startColumn - 1, row);
    }

    private void MoveCursorLeft() {
      if (CursorPosition == 0) {
        return;
      }
      if (ConsoleBuffer.CursorLeft == 0) {
        --ConsoleBuffer.CursorTop;
        ConsoleBuffer.CursorLeft = ConsoleBuffer.BufferWidth - 1;
      } else {
        --ConsoleBuffer.CursorLeft;
      }
    }

    private void MoveCursorRight() {
      if (CursorPosition >= LineLength) {
        return;
      }
      if (ConsoleBuffer.CursorLeft >= ConsoleBuffer.BufferWidth - 1) {
        ++ConsoleBuffer.CursorTop;
        ConsoleBuffer.CursorLeft = 0;
      } else {
        ++ConsoleBuffer.CursorLeft;
      }
    }
  }
}