using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using static System.IO.Directory;
using static System.IO.SearchOption;

namespace Bud.IO {
  public class RecursiveDirFiles : IFiles {
    public string SourceDir { get; }
    public string FileFilter { get; }

    public RecursiveDirFiles(string sourceDir, string fileFilter) {
      SourceDir = sourceDir;
      FileFilter = fileFilter;
    }

    public IEnumerator<string> GetEnumerator()
      => EnumerateFiles(SourceDir, FileFilter, AllDirectories).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IObservable<IFiles> AsObservable() {
      return Observable.Create<IFiles>(observer => {
        var compositeDisposable = new CompositeDisposable();

        var fileSystemWatcher = new FileSystemWatcher(SourceDir, FileFilter) {
          IncludeSubdirectories = true, EnableRaisingEvents = true
        };
        compositeDisposable.Add(fileSystemWatcher);

        var fileSystemObservable = Observable.Merge(
          Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(handler => fileSystemWatcher.Created += handler, handler => fileSystemWatcher.Created -= handler),
          Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(handler => fileSystemWatcher.Deleted += handler, handler => fileSystemWatcher.Deleted -= handler),
          Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(handler => fileSystemWatcher.Changed += handler, handler => fileSystemWatcher.Changed -= handler));

        var observationDisposable = Observable
          .Return(this)
          .Concat(fileSystemObservable.Select(args => this))
          .Subscribe(observer);
        compositeDisposable.Add(observationDisposable);

        return compositeDisposable;
      });
    }
  }
}