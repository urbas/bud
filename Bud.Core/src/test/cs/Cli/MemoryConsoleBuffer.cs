namespace Bud.Cli {
  public class MemoryConsoleBuffer : IConsoleBuffer {
    private const int DefaultBufferWidth = 80;
    private const int DefaultBufferHeight = 1;
    private readonly char[][] CharBuffer = CreateCharacterBuffer(DefaultBufferWidth, DefaultBufferHeight);

    public virtual void Write(char character) {
      CharBuffer[CursorTop][CursorLeft] = character;
      ++CursorLeft;
      if (CursorLeft >= BufferWidth) {
        CursorLeft = 0;
        ++CursorTop;
      }
    }

    public virtual int CursorLeft { get; set; }

    public virtual int CursorTop { get; set; }

    public virtual int BufferWidth => DefaultBufferWidth;

    public virtual int BufferHeight => DefaultBufferHeight;

    public virtual void MoveArea(int sourceLeft,
                                 int sourceTop,
                                 int sourceWidth,
                                 int sourceHeight,
                                 int targetLeft,
                                 int targetTop) {
      var tempBuffer = CopyAreaToNewArray(sourceLeft, sourceTop, sourceWidth, sourceHeight);
      ZeroArea(sourceLeft, sourceTop, sourceWidth, sourceHeight);
      CopyFrom(tempBuffer, sourceWidth, sourceHeight, targetLeft, targetTop);
    }

    public char this[int left, int top] => CharBuffer[top][left];

    public char[] ToArray() => CopyAreaToNewArray(0, 0, BufferWidth, BufferHeight);

    private char[] CopyAreaToNewArray(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight) {
      var tempBuffer = new char[sourceHeight * sourceWidth];
      for (int row = 0; row < sourceHeight; ++row) {
        for (int column = 0; column < sourceWidth; ++column) {
          tempBuffer[column + row * sourceWidth] = CharBuffer[row + sourceTop][column + sourceLeft];
          CharBuffer[row + sourceTop][column + sourceLeft] = '\0';
        }
      }
      return tempBuffer;
    }

    private static char[][] CreateCharacterBuffer(int bufferWidth, int bufferHeight) {
      var characterBuffer = new char[bufferHeight][];
      for (int bufferRow = 0; bufferRow < bufferHeight; bufferRow++) {
        characterBuffer[bufferRow] = new char[bufferWidth];
      }
      return characterBuffer;
    }

    private void CopyFrom(char[] sourceArray, int sourceWidth, int sourceHeight, int targetLeft, int targetTop) {
      for (int row = 0; row < sourceHeight; ++row) {
        for (int column = 0; column < sourceWidth; ++column) {
          CharBuffer[row + targetTop][column + targetLeft] = sourceArray[column + row * sourceWidth];
        }
      }
    }

    private void ZeroArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight) {
      for (int row = 0; row < sourceHeight; ++row) {
        for (int column = 0; column < sourceWidth; ++column) {
          CharBuffer[row + sourceTop][column + sourceLeft] = '\0';
        }
      }
    }
  }
}