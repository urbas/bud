using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bud.Util;

namespace Bud.Build.Macros {
  internal class WatchingTaskEvaluator {
    private const int WatchReactPeriodMillis = 300;
    private readonly IEnumerable<string> WatchedDirs;
    private readonly IEnumerable<string> WatchedTasks;
    private bool IsFileChanged;

    public WatchingTaskEvaluator(IEnumerable<string> watchedDirs, IEnumerable<string> watchedTasks) {
      WatchedDirs = watchedDirs;
      WatchedTasks = watchedTasks.ToArray();
    }

    public void StartWatching(IContext context) {
      InitializeFileWatchers();
      SpinWatch(context);
    }

    private void InitializeFileWatchers() {
      var watchers = WatchedDirs.Select(dir => new FileSystemWatcher(dir) {
        IncludeSubdirectories = true,
        EnableRaisingEvents = true
      });

      foreach (var watcher in watchers) {
        watcher.Changed += (sender, args) => IsFileChanged = true;
        watcher.Created += (sender, args) => IsFileChanged = true;
        watcher.Deleted += (sender, args) => IsFileChanged = true;
      }
    }

    private void SpinWatch(IContext context) {
      while (true) {
        if (IsFileChanged) {
          IsFileChanged = false;
          try {
            Task.WaitAll(WatchedTasks.Select(context.Evaluate).ToArray());
          } catch (Exception e) {
            ExceptionUtils.PrintItemizedErrorMessages(e, false);
          }
        }
        Thread.Sleep(WatchReactPeriodMillis);
      }
    }
  }
}