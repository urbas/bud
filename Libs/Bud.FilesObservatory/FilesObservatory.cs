using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Bud {
  public static class FilesObservatory {
    /// <returns>
    ///   an observable (the observatory) that produces a stream of files and directories
    ///   that have been changed.
    /// </returns>
    /// <param name="dir">
    ///   the directory in which to observe changes.
    /// </param>
    /// <param name="fileFilter">
    ///   only files that match this filter will be tracked. You can use the '*' wildcard. For example,
    ///   a filter of "<c>*.txt</c>" will watch all files with the "<c>.txt</c>" extension.
    /// </param>
    /// <param name="includeSubdirectories">
    ///   indicates whether changes in subdirectories
    ///   should be tracked too.
    /// </param>
    /// <param name="subscribedCallback">
    ///   this function will be called when the observatory starts watching the filesystem.
    ///   Note that the watching does not start immediatelly upon a call to this function.
    ///   The watching starts only when a subscriber registers itself to the returned observatory.
    /// </param>
    /// <param name="disposedCallback">
    ///   this function is called when the observatory stops watching the filesystem. This
    ///   happens when the last subscriber detaches itself from the returned observatory.
    /// </param>
    /// <remarks>
    ///   <para>
    ///     Important: this observable will publish its elements on the file system watcher's thread.
    ///     Please make sure you use
    ///     <see cref="Observable.ObserveOn{TSource}(IObservable{TSource},System.Reactive.Concurrency.IScheduler)" />
    ///     and observe on an appropriate scheduler.
    ///   </para>
    ///   <para>
    ///     This function returns a cold observable. The observable will start watching the filesystem
    ///     upon subscription. It will stop watching the filesystem when all subscribers unsubscribe.
    ///   </para>
    ///   <para>
    ///     The observable is reusable, which means that it can be subscribed to again after a complete
    ///     unsubscription.
    ///   </para>
    /// </remarks>
    public static IObservable<string> ObserveFileSystem(string dir,
                                                        string fileFilter,
                                                        bool includeSubdirectories,
                                                        Action subscribedCallback = null,
                                                        Action disposedCallback = null)
      => Observable.Create<string>(observer => {
        var compositeDisposable = new CompositeDisposable {
          new FileSystemObserver(dir, fileFilter, includeSubdirectories, observer),
          new CallbackDisposable(disposedCallback)
        };
        subscribedCallback?.Invoke();
        return compositeDisposable;
      }).Publish().RefCount();

    private class FileSystemObserver : IDisposable {
      private readonly IObserver<string> observer;
      private readonly FileSystemWatcher fileSystemWatcher;

      public FileSystemObserver(string watcherDir,
                                string fileFilter,
                                bool includeSubdirectories,
                                IObserver<string> observer) {
        this.observer = observer;
        fileSystemWatcher = new FileSystemWatcher(watcherDir, fileFilter) {
          IncludeSubdirectories = includeSubdirectories,
          EnableRaisingEvents = true
        };

        fileSystemWatcher.Created += OnFileChanged;
        fileSystemWatcher.Deleted += OnFileChanged;
        fileSystemWatcher.Changed += OnFileChanged;
        fileSystemWatcher.Renamed += OnFileRenamed;
      }

      private void OnFileRenamed(object sender, RenamedEventArgs args) {
        observer.OnNext(args.OldFullPath);
        observer.OnNext(args.FullPath);
      }

      private void OnFileChanged(object sender, FileSystemEventArgs args)
        => observer.OnNext(args.FullPath);

      public void Dispose() {
        fileSystemWatcher.Created -= OnFileChanged;
        fileSystemWatcher.Deleted -= OnFileChanged;
        fileSystemWatcher.Changed -= OnFileChanged;
        fileSystemWatcher.Renamed -= OnFileRenamed;
        fileSystemWatcher.EnableRaisingEvents = false;
        fileSystemWatcher.Dispose();
      }
    }

    private class CallbackDisposable : IDisposable {
      private readonly Action disposedCallback;

      public CallbackDisposable(Action disposedCallback) {
        this.disposedCallback = disposedCallback;
      }

      public void Dispose() => disposedCallback?.Invoke();
    }
  }
}