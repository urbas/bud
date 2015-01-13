using System;
using System.IO;
using Bud.IO;

namespace Bud.Test.Util {
  public class TemporaryDirectory : IDisposable {

    private readonly string temporaryDirectory;

    public TemporaryDirectory() {
      temporaryDirectory = Directories.CreateTemporary(string.Empty, string.Empty);
    }

    public TemporaryDirectory(string directoryToCopy) {
      if (!Directory.Exists(directoryToCopy)) {
        throw new ArgumentException(string.Format("The directory '{0}' does not exist.", directoryToCopy));
      }
      temporaryDirectory = Directories.CreateTemporary(System.IO.Path.GetFileName(directoryToCopy) + "-", string.Empty);
      Directories.Copy(directoryToCopy, temporaryDirectory);
    }

    public string Path {
      get { return temporaryDirectory; }
    }

    public void Dispose() {
      Directory.Delete(temporaryDirectory, true);
    }
  }
}