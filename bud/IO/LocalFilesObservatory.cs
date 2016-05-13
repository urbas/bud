using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Bud.IO {
  public class LocalFilesObservatory : IFilesObservatory {
    public IObservable<string> CreateObserver(string dir,
                                              string fileFilter,
                                              bool includeSubfolders)
      => ObserveFileSystem(dir, fileFilter, includeSubfolders);

    /// <returns>
    ///   an observable that produces a stream of all files and directories that have been changed.
    /// </returns>
    /// <remarks>
    ///   <para>
    ///     This function returns a cold observable. The observable will start watching the file system
    ///     upon subscription. It will stop watching the file system when all subscribers unsubscribe.
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