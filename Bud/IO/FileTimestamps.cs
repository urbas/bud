using System;
using System.IO;

namespace Bud.IO {
  public class FileTimestamps : IFileTimestamps {
    public static IFileTimestamps Instance { get; } = new FileTimestamps();
    public DateTime GetTimestamp(string file) => File.GetLastWriteTime(file);
  }
}