using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
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

        var observationDisposable = Observable
          .Merge(ObserveFileCreations(fileSystemWatcher),
                 ObserveFileDeletions(fileSystemWatcher),
                 ObserveFileChanges(fileSystemWatcher))
          .Select(pattern => pattern.EventArgs.FullPath)
          .Merge(ObserveFileRenames(fileSystemWatcher))
          .Subscribe(observer);
        compositeDisposable.Add(observationDisposable);

        return compositeDisposable;
      });

    private static IObservable<EventPattern<FileSystemEventArgs>> ObserveFileChanges(FileSystemWatcher fileSystemWatcher)
      => Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
        handler => fileSystemWatcher.Changed += handler,
        handler => fileSystemWatcher.Changed -= handler);

    private static IObservable<EventPattern<FileSystemEventArgs>> ObserveFileDeletions(FileSystemWatcher fileSystemWatcher)
      => Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
        handler => fileSystemWatcher.Deleted += handler,
        handler => fileSystemWatcher.Deleted -= handler);

    private static IObservable<EventPattern<FileSystemEventArgs>> ObserveFileCreations(FileSystemWatcher fileSystemWatcher)
      => Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
        handler => fileSystemWatcher.Created += handler,
        handler => fileSystemWatcher.Created -= handler);

    private static IObservable<string> ObserveFileRenames(FileSystemWatcher fileSystemWatcher)
      => Observable.FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
        handler => fileSystemWatcher.Renamed += handler,
        handler => fileSystemWatcher.Renamed -= handler)
                   .SelectMany(ToFileChanges);

    private static IEnumerable<string> ToFileChanges(EventPattern<RenamedEventArgs> pattern)
      => new[] {pattern.EventArgs.OldFullPath, pattern.EventArgs.FullPath};
  }
}