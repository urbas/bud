using System;
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
      infoLevelWriter.WriteLine(FormatMessage(InfoLevelId, message));
    }

    public void Error(string message) {
      errorLevelWriter.WriteLine(FormatMessage(ErrorLevelId, message));
    }

    private string FormatMessage(string level, string message) {
      return string.Format("[{0} {1}] {2}", level, DateTime.Now.ToString("HH:mm:ss.fff"), message);
    }
  }
}