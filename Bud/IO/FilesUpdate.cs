using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Bud.IO {
  public struct FilesUpdate : IFiles {
    public FilesUpdate(FileSystemEventArgs fileSystemEventArgs, IFiles files) {
      FileSystemEventArgs = fileSystemEventArgs;
      Files = files;
    }

    public FileSystemEventArgs FileSystemEventArgs { get; }
    public IFiles Files { get; }
    public IEnumerator<string> GetEnumerator() => Files.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Files).GetEnumerator();
    public IObservable<FilesUpdate> AsObservable() => Files.AsObservable();
  }
}