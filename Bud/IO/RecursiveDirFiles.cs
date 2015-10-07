using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using static System.IO.Directory;
using static System.IO.SearchOption;

namespace Bud.IO {
  public class RecursiveDirFiles : IFiles {
    public IFilesObservatory FilesObservatory { get; }
    public string SourceDir { get; }
    public string FileFilter { get; }

    public RecursiveDirFiles(IFilesObservatory filesObservatory, string sourceDir, string fileFilter) {
      FilesObservatory = filesObservatory;
      SourceDir = sourceDir;
      FileFilter = fileFilter;
    }

    public IEnumerator<string> GetEnumerator()
      => EnumerateFiles(SourceDir, FileFilter, AllDirectories).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IObservable<IFiles> AsObservable()
      => Observable.Return(this)
                   .Concat(FilesObservatory.CreateObserver(SourceDir, FileFilter, true).Select(pattern => this));
  }
}