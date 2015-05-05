using System;
using System.IO;

namespace Bud.IO {
  public class TemporaryDirectory : IDisposable {

    private readonly string TempDirPath;

    public TemporaryDirectory() {
      TempDirPath = Directories.CreateTemporary(string.Empty, string.Empty);
    }

    public TemporaryDirectory(string directoryToCopy) {
      if (!Directory.Exists(directoryToCopy)) {
        throw new ArgumentException(string.Format("The directory '{0}' does not exist.", directoryToCopy));
      }
      TempDirPath = Directories.CreateTemporary(System.IO.Path.GetFileName(directoryToCopy) + "-", string.Empty);
      try {
        Directories.Copy(directoryToCopy, TempDirPath);
      } catch (Exception) {
        Dispose();
        throw;
      }
    }

    public string Path => TempDirPath;

    public void Dispose() => Directory.Delete(TempDirPath, true);

    public override string ToString() => Path;
  }
}