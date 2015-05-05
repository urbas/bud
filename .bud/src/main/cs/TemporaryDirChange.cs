using System;
using System.IO;

public class TemporaryDirChange : IDisposable {
  private readonly string OldWorkingDir;

  public TemporaryDirChange(string dir) {
    OldWorkingDir = Directory.GetCurrentDirectory();
    Directory.SetCurrentDirectory(dir);
  }

  public void Dispose() {
    Directory.SetCurrentDirectory(OldWorkingDir);
  }
}