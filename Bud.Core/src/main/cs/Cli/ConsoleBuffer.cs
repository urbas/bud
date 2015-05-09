using System;

namespace Bud.Cli {
  public class ConsoleBuffer : IConsoleBuffer {
    public void Write(char character) => Console.Write(character);

    public int CursorLeft {
      get { return Console.CursorLeft; }
      set { Console.CursorLeft = value; }
    }

    public int CursorTop {
      get { return Console.CursorTop; }
      set { Console.CursorTop = value; }
    }

    public int BufferWidth => Console.BufferWidth;

    public void MoveArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop) {
      Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop);
    }
  }
}