using System;
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
    public IEnumerable<string> Enumerate() => Files.Enumerate();
    public IObservable<FilesUpdate> Watch() => Files.Watch();
  }
}