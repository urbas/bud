using System;
using System.Collections;
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

    public ListedFiles(IObservable<FileSystemEventArgs> filesObservableFactory, IEnumerable<string> files) {
      Files = files;
      FilesObserverFactory = () => filesObservableFactory.Select(ToFilesUpdate);
    }

    public ListedFiles(params string[] files) : this(Observable.Empty<FileSystemEventArgs>(), files) {}

    public IObservable<FilesUpdate> AsObservable() {
      return Observable.Return(ToFilesUpdate(null))
                       .Concat(FilesObserverFactory());
    }

    public IEnumerator<string> GetEnumerator() => Files.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Files).GetEnumerator();

    private FilesUpdate ToFilesUpdate(FileSystemEventArgs fileSystemEventArgs)
      => new FilesUpdate(fileSystemEventArgs, this);
  }
}