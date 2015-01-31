using System;
using System.IO;

namespace Bud.Logging {
  public class Logger {
    public static ILogger CreateFromWriters(TextWriter infoLevelWriter, TextWriter errorLevelWriter) {
      return new WriterLogger(infoLevelWriter, errorLevelWriter);
    }

    public static ILogger CreateFromStandardOutputs() {
      return CreateFromWriters(Console.Out, Console.Error);
    }
  }
}