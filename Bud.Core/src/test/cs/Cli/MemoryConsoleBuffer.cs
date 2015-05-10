using System;

namespace Bud.Cli {
  public class MemoryConsoleBuffer : IConsoleBuffer {
    public readonly char[] CharBuffer;
    private int CursorX;
    private int CursorY;

    private MemoryConsoleBuffer(int bufferWidth, int bufferHeight, char[] charBuffer) {
      BufferWidth = bufferWidth;
      BufferHeight = bufferHeight;
      CharBuffer = charBuffer;
    }

    public MemoryConsoleBuffer(int bufferWidth, int bufferHeight)
      : this(bufferWidth, bufferHeight, new char[bufferHeight * bufferWidth]) {}

    public MemoryConsoleBuffer(int bufferWidth, int bufferHeight, params string[] contentRows)
      : this(bufferWidth, bufferHeight, ConsoleBufferTestUtils.ToCharBuffer(bufferWidth, bufferHeight, contentRows)) {}

    public void Write(char character) {
      CharBuffer[CursorLeft + CursorTop * BufferWidth] = character;
      this.IncrementCursorPosition();
    }

    public int BufferWidth { get; }

    public int BufferHeight { get; }

    public int CursorLeft {
      get { return CursorX; }
      set {
        AssertWidthIsInRange(value);
        CursorX = value;
      }
    }

    public int CursorTop {
      get { return CursorY; }
      set {
        AssertHeightIsInRange(value);
        CursorY = value;
      }
    }

    public char this[int column, int row] {
      get { return CharBuffer[column + BufferWidth * row]; }
      private set { CharBuffer[column + BufferWidth * row] = value; }
    }

    public void MoveArea(int sourceLeft,
                         int sourceTop,
                         int sourceWidth,
                         int sourceHeight,
                         int targetLeft,
                         int targetTop) {
      var tempBuffer = CopyAreaToNewArray(sourceLeft, sourceTop, sourceWidth, sourceHeight);
      ZeroArea(sourceLeft, sourceTop, sourceWidth, sourceHeight);
      CopyFrom(tempBuffer, sourceWidth, sourceHeight, targetLeft, targetTop);
    }

    private char[] CopyAreaToNewArray(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight) {
      var tempBuffer = new char[sourceHeight * sourceWidth];
      for (int row = 0; row < sourceHeight; ++row) {
        for (int column = 0; column < sourceWidth; ++column) {
          tempBuffer[column + row * sourceWidth] = this[column + sourceLeft, row + sourceTop];
          this[column + sourceLeft, row + sourceTop] = '\0';
        }
      }
      return tempBuffer;
    }

    private void CopyFrom(char[] sourceArray, int sourceWidth, int sourceHeight, int targetLeft, int targetTop) {
      for (int row = 0; row < sourceHeight; ++row) {
        for (int column = 0; column < sourceWidth; ++column) {
          this[column + targetLeft, row + targetTop] = sourceArray[column + row * sourceWidth];
        }
      }
    }

    private void ZeroArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight) {
      for (int row = 0; row < sourceHeight; ++row) {
        for (int column = 0; column < sourceWidth; ++column) {
          this[column + sourceLeft, row + sourceTop] = '\0';
        }
      }
    }

    private void AssertWidthIsInRange(int width) {
      if (width < 0 || width >= BufferWidth) {
        throw new IndexOutOfRangeException();
      }
    }

    private void AssertHeightIsInRange(int height) {
      if (height < 0 || height >= BufferHeight) {
        throw new IndexOutOfRangeException();
      }
    }
  }
}