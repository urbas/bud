using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;

namespace Bud.IO {
  public class FileTimestamps : IFileTimestamps {
    public static IFileTimestamps Instance { get; } = new FileTimestamps();
    public DateTime GetTimestamp(string file) => File.GetLastWriteTime(file);

    public static IEnumerable<Timestamped<string>> ToTimestampedFiles(IEnumerable<string> files)
      => files.Select(s => new Timestamped<string>(s, Instance.GetTimestamp(s)));
  }
}