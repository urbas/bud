using System;

namespace Bud.Cli {
  public class MemoryConsoleBuffer : IConsoleBuffer {
    private readonly char[][] CharBuffer;
    private int CursorX;
    private int CursorY;

    public MemoryConsoleBuffer(int bufferWidth, int bufferHeight) {
      BufferWidth = bufferWidth;
      BufferHeight = bufferHeight;
      CharBuffer = CreateCharacterBuffer(BufferWidth, BufferHeight);
    }

    public void Write(char character) {
      CharBuffer[CursorTop][CursorLeft] = character;
      if (CursorLeft >= BufferWidth - 1) {
        CursorLeft = 0;
        ++CursorTop;
      } else {
        ++CursorLeft;
      }
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