namespace Bud.Cli {
  public interface IConsoleBuffer {
    int CursorLeft { get; set; }
    int CursorTop { get; set; }
    int BufferWidth { get; }
    int BufferHeight { get; }
    IConsoleBuffer MoveArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop);
    IConsoleBuffer Write(char character);
    IConsoleBuffer WriteLine(string str);
    IConsoleBuffer Write(string str);
    IConsoleBuffer WriteLine();
  }
}