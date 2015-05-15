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
    private bool HasFolderChanged = true;

    public WatchingTaskEvaluator(IEnumerable<string> watchedDirs, IEnumerable<string> watchedTasks) {
      if (!watchedTasks.Any()) {
        throw new ArgumentException("No tasks to watch.");
      }
      WatchedDirs = watchedDirs;
      WatchedTasks = watchedTasks.ToArray();
    }

    public void StartWatching(IBuildContext context) {
      InitializeFileWatchers();
      SpinWatch(context);
    }

    private void InitializeFileWatchers() {
      var watchers = WatchedDirs.Select(dir => new FileSystemWatcher(dir) {
        IncludeSubdirectories = true,
        EnableRaisingEvents = true
      });

      foreach (var watcher in watchers) {
        watcher.Changed += (sender, args) => HasFolderChanged = true;
        watcher.Created += (sender, args) => HasFolderChanged = true;
        watcher.Deleted += (sender, args) => HasFolderChanged = true;
      }
    }


    private void SpinWatch(IBuildContext buildContext) {
      while (true) {
        if (HasFolderChanged) {
          HasFolderChanged = false;
          try {
            var context = buildContext.Context;
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