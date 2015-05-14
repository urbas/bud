using System;

namespace Bud.Cli {
  public class ConsoleBuffer : IConsoleBuffer {
    public int CursorLeft {
      get { return Console.CursorLeft; }
      set { Console.CursorLeft = value; }
    }

    public int CursorTop {
      get { return Console.CursorTop; }
      set { Console.CursorTop = value; }
    }

    public int BufferWidth => Console.BufferWidth;

    public int BufferHeight => Console.BufferHeight;

    public IConsoleBuffer MoveArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop) {
      Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop);
      return this;
    }

    public IConsoleBuffer Write(char character) {
      Console.Write(character);
      return this;
    }

    public IConsoleBuffer WriteLine(string str) {
      Console.WriteLine(str);
      return this;
    }

    public IConsoleBuffer Write(string str) {
      Console.Write(str);
      return this;
    }

    public IConsoleBuffer WriteLine() {
      Console.WriteLine();
      return this;
    }
  }
}