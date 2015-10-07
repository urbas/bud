using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public class ListedFiles : IFiles {
    public IEnumerable<string> Files { get; }
    public IFileSystemObserverFactory FileSystemObserverFactory { get; }

    public ListedFiles(IFileSystemObserverFactory fileSystemObserverFactory, IEnumerable<string> files) {
      Files = files;
      FileSystemObserverFactory = fileSystemObserverFactory;
    }

    public IObservable<IFiles> AsObservable() {
      var filesObservable = Files.Select(s => FileSystemObserverFactory.Create(Path.GetDirectoryName(s), Path.GetFileName(s), false)).Merge().Select(args => this);
      return Observable.Return(this).Concat(filesObservable);
    }

    public IEnumerator<string> GetEnumerator() => Files.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Files).GetEnumerator();
  }
}