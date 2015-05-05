using System;
using System.IO;

namespace Bud.IO {
  public class TemporaryDirChange : IDisposable {
    public readonly string OldWorkingDir;

    public TemporaryDirChange(string dir) {
      OldWorkingDir = Directory.GetCurrentDirectory();
      Directory.SetCurrentDirectory(dir);
    }

    public void Dispose() {
      Directory.SetCurrentDirectory(OldWorkingDir);
    }
  }
}