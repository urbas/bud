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
        ConsoleBuffer.ShiftBufferRight(ConsoleBuffer.CursorLeft,
                                       ConsoleBuffer.CursorTop,
                                       LineLength - CursorPosition);
      }
      LineBuffer.Insert(CursorPosition, character);
      ConsoleBuffer.Write(character);
    }

    private void DeleteCurrentCharacter() {
      if (CursorPosition == 0) {
        return;
      }
      LineBuffer.Remove(CursorPosition - 1, 1);
      MoveCursorLeft();
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
  }
}