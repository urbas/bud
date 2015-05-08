using System;

namespace Bud.Cli {
  public class ConsoleBuffer {
    public void Write(char character) => Console.Write(character);

    public int CursorLeft {
      get { return Console.CursorLeft; }
      set { Console.CursorLeft = value; }
    }

    public int CursorTop
    {
      get { return Console.CursorTop; }
      set { Console.CursorTop = value; }
    }

    public int BufferWidth => Console.BufferWidth;
  }
}