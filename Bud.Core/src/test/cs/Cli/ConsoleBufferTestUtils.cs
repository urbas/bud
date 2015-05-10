using System;

namespace Bud.Cli {
  public static class ConsoleBufferTestUtils {
    public static char[] ToCharBuffer(int bufferWidth, int bufferHeight, params string[] bufferRows) {
      var buffer = new char[bufferWidth * bufferHeight];
      for (int rowIndex = 0; rowIndex < bufferRows.Length; rowIndex++) {
        Array.Copy(bufferRows[rowIndex].ToCharArray(), 0, buffer, rowIndex * bufferWidth, bufferRows[rowIndex].Length);
      }
      return buffer;
    }

    public static ConsoleKeyInfo ToInput(ConsoleKey consoleKey) {
      return new ConsoleKeyInfo((char) consoleKey, consoleKey, false, false, false);
    }
  }
}