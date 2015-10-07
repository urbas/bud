using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public class ListedFiles : IFiles {
    public IEnumerable<string> Files { get; }
    public IFilesObservatory FilesObservatory { get; }

    public ListedFiles(IFilesObservatory filesObservatory, IEnumerable<string> files) {
      Files = files;
      FilesObservatory = filesObservatory;
    }

    public IObservable<IFiles> AsObservable() {
      var filesObservable = Files.Select(s => FilesObservatory.CreateObserver(Path.GetDirectoryName(s), Path.GetFileName(s), false)).Merge().Select(args => this);
      return Observable.Return(this).Concat(filesObservable);
    }

    public IEnumerator<string> GetEnumerator() => Files.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Files).GetEnumerator();
  }
}