using System;
using System.Text;

namespace Bud.Cli {
  public class SingleLineEditor {
    private readonly IConsoleBuffer ConsoleBuffer;
    private readonly StringBuilder LineBuffer = new StringBuilder();

    public SingleLineEditor(IConsoleBuffer consoleBuffer) {
      ConsoleBuffer = consoleBuffer;
    }

    public string Line => LineBuffer.ToString();

    public void ProcessInput(ConsoleKeyInfo consoleKeyInfo) {
      if (consoleKeyInfo.Key == ConsoleKey.Backspace) {
        DeleteCurrentCharacter();
      } else if (IsKeyInRange(consoleKeyInfo.Key, ConsoleKey.PageUp, ConsoleKey.DownArrow)) {
        MoveCursorBackwards();
      } else {
        AppendCharacter(consoleKeyInfo.KeyChar);
      }
    }

    private bool IsLineEmpty => LineBuffer.Length == 0;

    private static bool IsKeyInRange(ConsoleKey key,
                                     ConsoleKey lowerBound,
                                     ConsoleKey upperBound)
      => lowerBound <= key && key <= upperBound;

    private void AppendCharacter(char character) {
      if (ConsoleBuffer.CursorLeft < LineBuffer.Length) {
        PushCharactersAfterCursor();
      }
      LineBuffer.Insert(ConsoleBuffer.CursorLeft, character);
      ConsoleBuffer.Write(character);
    }

    private void PushCharactersAfterCursor() {
      var numberOfCharactersToMove = LineBuffer.Length - ConsoleBuffer.CursorLeft;
      ConsoleBuffer.MoveArea(ConsoleBuffer.CursorLeft,
                             ConsoleBuffer.CursorTop,
                             numberOfCharactersToMove,
                             1,
                             ConsoleBuffer.CursorLeft + 1, ConsoleBuffer.CursorTop);
    }

    private void DeleteCurrentCharacter() {
      if (IsLineEmpty) {
        return;
      }
      LineBuffer.Remove(ConsoleBuffer.CursorLeft - 1, 1);
      MoveCursorBackwards();
      ConsoleBuffer.MoveArea(ConsoleBuffer.CursorLeft + 1,
                             ConsoleBuffer.CursorTop,
                             LineBuffer.Length - ConsoleBuffer.CursorLeft + 1,
                             1,
                             ConsoleBuffer.CursorLeft,
                             ConsoleBuffer.CursorTop);
    }

    private void MoveCursorBackwards() {
      if (ConsoleBuffer.CursorLeft <= 0) {
        return;
      }
      ConsoleBuffer.CursorLeft = ConsoleBuffer.CursorLeft - 1;
    }

    public static SingleLineEditor Create() => new SingleLineEditor(new ConsoleBuffer());
  }
}