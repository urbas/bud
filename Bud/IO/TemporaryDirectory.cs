using System;
using System.IO;
using System.Linq;

namespace Bud.IO {
  public class TemporaryDirectory : IDisposable {
    public TemporaryDirectory() {
      Path = CreateDirectory();
    }

    public string Path { get; }

    public void Dispose() => Directory.Delete(Path, true);

    public override string ToString() => Path;

    private static string CreateDirectory() {
      string baseDir = System.IO.Path.GetTempPath();
      string tempDir;
      do {
        tempDir = System.IO.Path.Combine(baseDir, Guid.NewGuid().ToString());
      } while (Directory.Exists(tempDir));
      Directory.CreateDirectory(tempDir);
      return tempDir;
    }

    public string CreateFile(params string[] subPath) {
      var file = System.IO.Path.Combine(new[] {Path}.Concat(subPath).ToArray());
      Directory.CreateDirectory(System.IO.Path.GetDirectoryName(file));
      File.Create(file).Dispose();
      return file;
    }
  }
}