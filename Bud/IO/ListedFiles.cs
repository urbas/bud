using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using static System.IO.Path;

namespace Bud.IO {
  public class ListedFiles : IFiles {
    public Func<IObservable<FilesUpdate>> FilesObserverFactory { get; }
    public IEnumerable<string> Files { get; }

    public ListedFiles(IFilesObservatory filesObservatory, IEnumerable<string> files) {
      Files = files;
      FilesObserverFactory = () => Files.Select(file => filesObservatory.CreateObserver(GetDirectoryName(file), GetFileName(file), false)).Merge().Select(ToFilesUpdate);
    }

    public ListedFiles(IObservable<FileSystemEventArgs> filesObservable, IEnumerable<string> files) {
      Files = files;
      FilesObserverFactory = () => filesObservable.Select(ToFilesUpdate);
    }

    public ListedFiles(params string[] files) : this(Observable.Empty<FileSystemEventArgs>(), files) {}

    public IEnumerable<string> Enumerate() => Files;

    public IObservable<FilesUpdate> Watch()
      => Observable.Return(ToFilesUpdate(null))
                   .Concat(FilesObserverFactory());

    private FilesUpdate ToFilesUpdate(FileSystemEventArgs fileSystemEventArgs)
      => new FilesUpdate(fileSystemEventArgs, this);
  }
}