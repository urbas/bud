using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Bud.IO {
  public class LocalFilesObservatory : IFilesObservatory {
    public IObservable<string> CreateObserver(string sourceDir, string fileFilter, bool includeSubfolders)
      => ObserveFileSystem(sourceDir, fileFilter, includeSubfolders);

    public static IObservable<string> ObserveFileSystem(string watcherDir,
                                                        string fileFilter,
                                                        bool includeSubdirectories,
                                                        Action subscribedCallback = null,
                                                        Action disposedCallback = null)
      => Observable.Create<string>(observer => {
        var fileSystemWatcher = new FileSystemWatcher(watcherDir, fileFilter) {
          IncludeSubdirectories = includeSubdirectories,
          EnableRaisingEvents = true
        };

        var blockingCollection = new BlockingCollection<string>();

        fileSystemWatcher.Created += (sender, args) => blockingCollection.Add(args.FullPath);
        fileSystemWatcher.Deleted += (sender, args) => blockingCollection.Add(args.FullPath);
        fileSystemWatcher.Changed += (sender, args) => blockingCollection.Add(args.FullPath);
        fileSystemWatcher.Renamed += (sender, args) => {
          blockingCollection.Add(args.OldFullPath);
          blockingCollection.Add(args.FullPath);
        };

        var compositeDisposable = new CompositeDisposable {
          fileSystemWatcher,
          new BlockingCollectionDisposable(blockingCollection),
          blockingCollection.GetConsumingEnumerable().ToObservable().Subscribe(observer),
          new CallbackDisposable(disposedCallback)
        };
        subscribedCallback?.Invoke();
        return compositeDisposable;
      });

    private class CallbackDisposable : IDisposable {
      private readonly Action disposedCallback;

      public CallbackDisposable(Action disposedCallback) {
        this.disposedCallback = disposedCallback;
      }

      public void Dispose() => disposedCallback?.Invoke();
    }

    public class BlockingCollectionDisposable : IDisposable {
      private readonly BlockingCollection<string> blockingCollection;

      public BlockingCollectionDisposable(BlockingCollection<string> blockingCollection) {
        this.blockingCollection = blockingCollection;
      }

      public void Dispose() => blockingCollection.CompleteAdding();
    }
  }
}