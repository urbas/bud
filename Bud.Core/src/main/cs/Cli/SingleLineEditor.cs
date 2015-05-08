using System;

namespace Bud.Cli {
  public class SingleLineEditor {
    private readonly ConsoleBuffer ConsoleBuffer;

    public SingleLineEditor(ConsoleBuffer consoleBuffer) {
      ConsoleBuffer = consoleBuffer;
    }

    public string Line { get; private set; }

    public void ProcessInput(ConsoleKeyInfo consoleKeyInfo) {
      if (consoleKeyInfo.Key == ConsoleKey.Backspace) {
        DeleteLastCharacter();
      } else {
        ConsoleBuffer.Write(consoleKeyInfo.KeyChar);
      }
    }

    private void DeleteLastCharacter() {
      MoveCursorBackwards();
      ConsoleBuffer.Write(' ');
      MoveCursorBackwards();
    }

    private void MoveCursorBackwards() {
      if (ConsoleBuffer.CursorLeft <= 0) {
        if (ConsoleBuffer.CursorTop <= 0) {
          return;
        }
        ConsoleBuffer.CursorLeft = ConsoleBuffer.BufferWidth - 1;
        ConsoleBuffer.CursorTop = ConsoleBuffer.CursorTop - 1;
      } else {
        ConsoleBuffer.CursorLeft = ConsoleBuffer.CursorLeft - 1;
      }
    }
  }
}