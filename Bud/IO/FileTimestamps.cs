using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;

namespace Bud.IO {
  public class FileTimestamps : IFileTimestamps {
    public DateTime GetTimestamp(string file) => File.GetLastWriteTime(file);

    public static IEnumerable<System.Reactive.Timestamped<string>> ToTimestampedFiles(IEnumerable<string> files)
      => files.Select(s => new System.Reactive.Timestamped<string>(s, File.GetLastWriteTime(s)));
  }
}