using System;
using System.IO;
using System.Linq;
using static System.IO.Directory;
using static System.IO.Path;

namespace Bud.IO {
  public class TemporaryDirectory : IDisposable {
    public TemporaryDirectory() {
      Path = CreateDirectoryInTempDir();
    }

    public string Path { get; }

    public void Dispose() => Delete(Path, true);

    public override string ToString() => Path;

    public string CreateEmptyFile(string firstPathComponent, params string[] pathComponents)
      => CreateFile(string.Empty, firstPathComponent, pathComponents);

    public string CreateFile(string content, string firstPathComponent, params string[] pathComponents) {
      var file = Combine(new[] {Path, firstPathComponent}.Concat(pathComponents).ToArray());
      CreateDirectory(GetDirectoryName(file));
      File.WriteAllText(file, content);
      return file;
    }

    private static string CreateDirectoryInTempDir() {
      string baseDir = GetTempPath();
      string tempDir;
      do {
        tempDir = Combine(baseDir, Guid.NewGuid().ToString());
      } while (Exists(tempDir));
      CreateDirectory(tempDir);
      return tempDir;
    }
  }
}