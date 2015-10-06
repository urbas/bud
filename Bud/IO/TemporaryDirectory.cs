using System;
using System.IO;
using System.Linq;
using static System.IO.Path;

namespace Bud.IO {
  public class TemporaryDirectory : IDisposable {
    public TemporaryDirectory() {
      Path = CreateDirectory();
    }

    public string Path { get; }

    public void Dispose() => Directory.Delete(Path, true);

    public override string ToString() => Path;

    private static string CreateDirectory() {
      string baseDir = GetTempPath();
      string tempDir;
      do {
        tempDir = Combine(baseDir, Guid.NewGuid().ToString());
      } while (Directory.Exists(tempDir));
      Directory.CreateDirectory(tempDir);
      return tempDir;
    }

    public string CreateEmptyFile(string firstPathComponent, params string[] pathComponents)
      => CreateFile(string.Empty, firstPathComponent, pathComponents);

    public string CreateFile(string content, string firstPathComponent, params string[] pathComponents) {
      var file = Combine(new[] {Path, firstPathComponent}.Concat(pathComponents).ToArray());
      Directory.CreateDirectory(GetDirectoryName(file));
      File.WriteAllText(file, content);
      return file;
    }
  }
}