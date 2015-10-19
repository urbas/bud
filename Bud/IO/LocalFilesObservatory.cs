using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Bud.IO {
  public class LocalFilesObservatory : IFilesObservatory {
    public IObservable<string> CreateObserver(string sourceDir, string fileFilter, bool includeSubfolders)
      => ObserveFileSystem(sourceDir, fileFilter, includeSubfolders);

    public static IObservable<string> ObserveFileSystem(string watcherDir,
                                                        string fileFilter,
                                                        bool includeSubdirectories)
      => Observable.Create<string>(observer => {
        var compositeDisposable = new CompositeDisposable();

        var fileSystemWatcher = new FileSystemWatcher(watcherDir, fileFilter) {
          IncludeSubdirectories = includeSubdirectories,
          EnableRaisingEvents = true
        };
        compositeDisposable.Add(fileSystemWatcher);

        var renameEvents = Observable.FromEventPattern<RenamedEventHandler, RenamedEventArgs>(handler => fileSystemWatcher.Renamed += handler, handler => fileSystemWatcher.Renamed -= handler)
                                     .SelectMany(pattern => new[] {pattern.EventArgs.OldFullPath, pattern.EventArgs.FullPath});
        var observationDisposable = Observable
          .Merge(Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(handler => fileSystemWatcher.Created += handler, handler => fileSystemWatcher.Created -= handler),
                 Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(handler => fileSystemWatcher.Deleted += handler, handler => fileSystemWatcher.Deleted -= handler),
                 Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(handler => fileSystemWatcher.Changed += handler, handler => fileSystemWatcher.Changed -= handler))
          .Select(pattern => pattern.EventArgs.FullPath)
          .Merge(renameEvents)
          .Subscribe(observer);
        compositeDisposable.Add(observationDisposable);

        return compositeDisposable;
      });
  }
}