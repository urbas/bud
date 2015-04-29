using System.IO;
using System.Threading.Tasks;

namespace Bud.IO {
  public static class TextWriterUtils {
    public const int PipeBufferSize = 4096;

    public static async Task PipeReadAsyncWriteSync(TextReader inputReader, TextWriter outputWriter) {
      var buffer = new char[PipeBufferSize];
      while (true) {
        var readChars = await inputReader.ReadAsync(buffer, 0, PipeBufferSize);
        if (readChars > 0) {
          outputWriter.Write(buffer, 0, readChars);
        } else {
          break;
        }
      }
    }
  }
}