namespace Bud.Cli {
  public interface IConsoleBuffer {
    void Write(char character);
    int CursorLeft { get; set; }
    int CursorTop { get; set; }
    int BufferWidth { get; }
    int BufferHeight { get; }
    void MoveArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop);
  }
}