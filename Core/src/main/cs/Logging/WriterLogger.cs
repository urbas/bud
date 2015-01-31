using System;
using System.Globalization;
using System.IO;

namespace Bud.Logging {
  public class WriterLogger : ILogger {
    private const string ErrorLevelId = "Error";
    private const string InfoLevelId = "Info";
    private readonly TextWriter infoLevelWriter;
    private readonly TextWriter errorLevelWriter;

    public WriterLogger(TextWriter infoLevelWriter, TextWriter errorLevelWriter) {
      this.infoLevelWriter = infoLevelWriter;
      this.errorLevelWriter = errorLevelWriter;
    }

    public void Info(string message) {
      PrintMessageHeader(InfoLevelId, infoLevelWriter);
      infoLevelWriter.WriteLine(message);
    }

    public void Error(string message) {
      PrintMessageHeader(ErrorLevelId, errorLevelWriter);
      errorLevelWriter.WriteLine(message);
    }

    private void PrintMessageHeader(string level, TextWriter writer) {
      writer.Write("[");
      writer.Write(level);
      writer.Write(" ");
      writer.Write(DateTime.Now.ToString("HH:mm:ss.fff"));
      writer.Write("] ");
    }
  }
}