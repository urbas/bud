using System;
using System.Text;

namespace Bud.Cli {
  public class SingleLineEditor {
    private readonly IConsoleBuffer ConsoleBuffer;
    private readonly StringBuilder LineBuffer = new StringBuilder();
    private int LastKnownBufferWidth;

    public SingleLineEditor(IConsoleBuffer consoleBuffer) {
      ConsoleBuffer = consoleBuffer;
      CursorStartLeft = consoleBuffer.CursorLeft;
      CursorStartTop = consoleBuffer.CursorTop;
      LastKnownBufferWidth = consoleBuffer.BufferWidth;
    }

    public string Line => LineBuffer.ToString();

    public int LineLength => LineBuffer.Length;

    public int CursorPosition { get; private set; }

    private int CursorStartLeft { get; set; }

    private int CursorStartTop { get; }

    public void ProcessInput(ConsoleKeyInfo consoleKeyInfo) {
      RefreshBufferLayout();
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
      ConsoleBuffer.Write(character);
      LineBuffer.Insert(CursorPosition, character);
      ++CursorPosition;
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
      --CursorPosition;
      ConsoleBuffer.DecrementCursorPosition();
    }

    private void MoveCursorRight() {
      if (CursorPosition >= LineLength) {
        return;
      }
      ++CursorPosition;
      ConsoleBuffer.IncrementCursorPosition();
    }

    private void MoveCursorToStart() {
      CursorPosition = 0;
      ResetBufferCursor(CursorPosition);
    }

    private void MoveCursorToEnd() {
      CursorPosition = LineLength;
      ResetBufferCursor(CursorPosition);
    }

    private void ResetBufferCursor(int cursorPosition) {
      ConsoleBuffer.CursorLeft = (CursorStartLeft + cursorPosition) % ConsoleBuffer.BufferWidth;
      ConsoleBuffer.CursorTop = CursorStartTop + (CursorStartLeft + cursorPosition) / ConsoleBuffer.BufferWidth;
    }

    private void RefreshBufferLayout() {
      if (ConsoleBuffer.BufferWidth != LastKnownBufferWidth) {
        if (CursorStartLeft >= ConsoleBuffer.BufferWidth - 1) {
          CursorStartLeft = 0;
        }
        ResetBufferCursor(0);
        for (int charIndex = 0; charIndex < LineLength; charIndex++) {
          ConsoleBuffer.Write(LineBuffer[charIndex]);
        }
        ResetBufferCursor(CursorPosition);
        LastKnownBufferWidth = ConsoleBuffer.BufferWidth;
      }
    }
  }
}