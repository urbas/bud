using System;
using System.IO;

namespace Bud.IO {
  public class TemporaryDirectory : IDisposable {
    public TemporaryDirectory() {
      Path = Directories.CreateTemporary(string.Empty, string.Empty);
    }

    public TemporaryDirectory(string directoryToCopy) {
      if (!Directory.Exists(directoryToCopy)) {
        throw new ArgumentException($"The directory '{directoryToCopy}' does not exist.");
      }
      Path = Directories.CreateTemporary(System.IO.Path.GetFileName(directoryToCopy) + "-", string.Empty);
      try {
        Directories.Copy(directoryToCopy, Path);
      } catch (Exception) {
        Dispose();
        throw;
      }
    }

    public string Path { get; }

    public void Dispose() => Directory.Delete(Path, true);

    public override string ToString() => Path;
  }
}