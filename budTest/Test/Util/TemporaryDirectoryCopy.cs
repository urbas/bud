using NUnit.Framework;
using System;
using System.IO;
using Bud.IO;

namespace Bud.Test.Util {
  public class TemporaryDirectoryCopy : IDisposable {

    private readonly string temporaryDirectory;

    public TemporaryDirectoryCopy(string directory) {
      if (!Directory.Exists(directory)) {
        throw new ArgumentException(string.Format("The directory '{0}' does not exist.", directory));
      }
      temporaryDirectory = Directories.CreateTemporary(System.IO.Path.GetFileName(directory) + "-", string.Empty);
      Directories.Copy(directory, temporaryDirectory);
    }

    public string Path {
      get { return temporaryDirectory; }
    }

    public void Dispose() {
      Directory.Delete(temporaryDirectory, true);
    }
  }
}