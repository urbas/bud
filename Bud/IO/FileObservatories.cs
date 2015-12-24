using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using static Bud.IO.Watcher;

namespace Bud.IO {
  public static class FileObservatories {
    public static Watcher<IEnumerable<string>> WatchDir(this IFilesObservatory filesObservatory,
                                                        string sourceDir,
                                                        string fileFilter,
                                                        bool includeSubdirs)
      => Watch(FindFiles(sourceDir, fileFilter, includeSubdirs),
               filesObservatory.CreateObserver(sourceDir, fileFilter, includeSubdirs));

    public static Watcher<IEnumerable<string>> WatchFiles(this IFilesObservatory filesObservatory,
                                                          IEnumerable<string> absolutePaths)
      => WatchFiles(absolutePaths,
                    WatchersForFiles(filesObservatory, absolutePaths));

    public static Watcher<IEnumerable<string>> WatchFiles(this IFilesObservatory filesobservatory,
                                                          params string[] absolutePaths)
      => WatchFiles(filesobservatory,
                    absolutePaths as IEnumerable<string>);

    private static IEnumerable<string> FindFiles(string sourceDir,
                                                 string fileFilter,
                                                 bool includeSubdirs)
      => Directory.EnumerateFiles(sourceDir,
                                  fileFilter,
                                  ToSearchOption(includeSubdirs));

    private static SearchOption ToSearchOption(bool includeSubdirs)
      => includeSubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

    private static IObservable<IEnumerable<string>> WatchersForFiles(IFilesObservatory filesObservatory,
                                                                     IEnumerable<string> absolutePaths)
      => absolutePaths.Select(file => SingleFileWatcher(filesObservatory, file))
                      .Merge();

    private static IObservable<IEnumerable<string>> SingleFileWatcher(IFilesObservatory filesObservatory,
                                                                      string file)
      => filesObservatory.CreateObserver(Path.GetDirectoryName(file),
                                         Path.GetFileName(file),
                                         false);

    public static Watcher<IEnumerable<string>> WatchFiles(IEnumerable<string> files,
                                                          IObservable<IEnumerable<string>> changes)
      => new Watcher<IEnumerable<string>>(files, changes);
  }
}