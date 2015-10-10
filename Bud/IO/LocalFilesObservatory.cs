using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Bud.IO {
  public class LocalFilesObservatory : IFilesObservatory {
    public IObservable<FileSystemEventArgs> CreateObserver(string sourceDir, string fileFilter, bool includeSubfolders)
      => ObserveFileSystem(sourceDir, fileFilter, includeSubfolders);

    public static IObservable<FileSystemEventArgs> ObserveFileSystem(string watcherDir,
                                                                     string fileFilter,
                                                                     bool includeSubdirectories)
      => Observable.Create<FileSystemEventArgs>(observer => {
        var compositeDisposable = new CompositeDisposable();

        var fileSystemWatcher = new FileSystemWatcher(watcherDir, fileFilter) {
          IncludeSubdirectories = includeSubdirectories,
          EnableRaisingEvents = true
        };
        compositeDisposable.Add(fileSystemWatcher);

        var observationDisposable = Observable
          .Merge(Observable.FromEventPattern<RenamedEventHandler, FileSystemEventArgs>(handler => fileSystemWatcher.Renamed += handler, handler => fileSystemWatcher.Renamed -= handler),
                 Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(handler => fileSystemWatcher.Created += handler, handler => fileSystemWatcher.Created -= handler),
                 Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(handler => fileSystemWatcher.Deleted += handler, handler => fileSystemWatcher.Deleted -= handler),
                 Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(handler => fileSystemWatcher.Changed += handler, handler => fileSystemWatcher.Changed -= handler))
          .Select(pattern => pattern.EventArgs)
          .Subscribe(observer);
        compositeDisposable.Add(observationDisposable);

        return compositeDisposable;
      });
  }
}