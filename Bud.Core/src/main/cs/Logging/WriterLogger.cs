using System;
using System.IO;

namespace Bud.Logging {
  public class WriterLogger : ILogger {
    private const string ErrorLevelId = "error";
    private const string InfoLevelId = "info";
    private readonly TextWriter InfoLevelWriter;
    private readonly TextWriter ErrorLevelWriter;

    public WriterLogger(TextWriter infoLevelWriter, TextWriter errorLevelWriter) {
      InfoLevelWriter = infoLevelWriter;
      ErrorLevelWriter = errorLevelWriter;
    }

    public void Info(string message) => InfoLevelWriter.WriteLine(FormatMessage(InfoLevelId, message));

    public void Error(string message) => ErrorLevelWriter.WriteLine(FormatMessage(ErrorLevelId, message));

    private static string FormatMessage(string level, string message) {
      return $"[{level} {DateTime.Now.ToString("HH:mm:ss.fff")}] {message}";
    }
  }
}