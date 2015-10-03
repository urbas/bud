using System;
using System.IO;
using Bud.IO;

namespace Bud.Logging {
  public class Logger {
    public static ILogger CreateFromWriters(TextWriter infoLevelWriter, TextWriter errorLevelWriter) {
      return new WriterLogger(infoLevelWriter, errorLevelWriter);
    }

    public static ILogger CreateFromStandardOutputs() => CreateFromWriters(Console.Out, Console.Error);
    public static readonly ILogger NullLogger = CreateFromWriters(NullTextWriter.Instance, NullTextWriter.Instance);
  }
}