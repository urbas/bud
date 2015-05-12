using System;

namespace Bud.Cli {
  public class MemoryConsoleBuffer : IConsoleBuffer {
    private int cursorLeft;
    private int cursorTop;
    private int bufferWidth;

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
      if (CursorLeft >= BufferWidth - 1) {
        ++CursorTop;
        CursorLeft = 0;
      } else {
        ++CursorLeft;
      }
    }

    public char[] CharBuffer { get; private set; }

    public int BufferWidth {
      get { return bufferWidth; }
      set {
        if (bufferWidth != value) {
          if (value < bufferWidth) {
            DecreaseBufferWidth(value);
          } else {
            IncreaseBufferWidth(value);
          }
        }
      }
    }

    public int BufferHeight { get; }

    public int CursorLeft {
      get { return cursorLeft; }
      set {
        AssertWidthIsInRange(value);
        cursorLeft = value;
      }
    }

    public int CursorTop {
      get { return cursorTop; }
      set {
        AssertHeightIsInRange(value);
        cursorTop = value;
      }
    }

    public char this[int column, int row] {
      get {
        AssertWidthIsInRange(column);
        AssertHeightIsInRange(row);
        return CharBuffer[column + BufferWidth * row];
      }
      private set {
        AssertWidthIsInRange(column);
        AssertHeightIsInRange(row);
        CharBuffer[column + BufferWidth * row] = value;
      }
    }

    public void MoveArea(int sourceLeft,
                         int sourceTop,
                         int sourceWidth,
                         int sourceHeight,
                         int targetLeft,
                         int targetTop) {
      var tempBuffer = CopyToNewArray(sourceLeft, sourceTop, sourceWidth, sourceHeight);
      ZeroFill(sourceLeft, sourceTop, sourceWidth, sourceHeight);
      CopyFrom(tempBuffer, sourceWidth, sourceHeight, targetLeft, targetTop);
    }

    private char[] CopyToNewArray(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight) {
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

    private void ZeroFill(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight) {
      for (int row = 0; row < sourceHeight; ++row) {
        for (int column = 0; column < sourceWidth; ++column) {
          this[column + sourceLeft, row + sourceTop] = '\0';
        }
      }
    }

    private void DecreaseBufferWidth(int newBufferWidth) {
      CharBuffer = CopyToNewArray(0, 0, newBufferWidth, BufferHeight);
      bufferWidth = newBufferWidth;
      if (CursorLeft >= newBufferWidth) {
        CursorLeft = 0;
        ++CursorTop;
      }
    }

    private void IncreaseBufferWidth(int newBufferWidth) {
      var oldBuffer = CharBuffer;
      var oldBufferWidth = BufferWidth;
      bufferWidth = newBufferWidth;
      CharBuffer = new char[BufferHeight * BufferWidth];
      CopyFrom(oldBuffer, oldBufferWidth, BufferHeight, 0, 0);
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