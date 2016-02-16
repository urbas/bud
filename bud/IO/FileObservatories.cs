using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using static System.IO.Path;

namespace Bud.IO {
  public static class FileObservatories {
    public static FileWatcher WatchDir(this IFilesObservatory filesObservatory,
                                       string sourceDir,
                                       string fileFilter,
                                       bool includeSubdirs)
      => new FileWatcher(FindFiles(sourceDir, fileFilter, includeSubdirs),
                         filesObservatory.CreateObserver(sourceDir, fileFilter, includeSubdirs));

    public static FileWatcher WatchFiles(this IFilesObservatory filesObservatory,
                                         IEnumerable<string> absolutePaths)
      => new FileWatcher(absolutePaths,
                         FilesChangesObserver(filesObservatory, absolutePaths));

    public static FileWatcher WatchFiles(this IFilesObservatory filesobservatory,
                                         params string[] absolutePaths)
      => WatchFiles(filesobservatory,
                    absolutePaths as IEnumerable<string>);

    public static IEnumerable<string> FindFiles(string sourceDir,
                                                string fileFilter,
                                                bool includeSubdirs)
      => Directory.EnumerateFiles(sourceDir,
                                  fileFilter,
                                  ToSearchOption(includeSubdirs));

    private static SearchOption ToSearchOption(bool includeSubdirs)
      => includeSubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

    private static IObservable<string> FilesChangesObserver(IFilesObservatory filesObservatory,
                                                            IEnumerable<string> absolutePaths)
      => absolutePaths.Select(file => SingleFileWatcher(filesObservatory, file))
                      .Merge();

    private static IObservable<string> SingleFileWatcher(IFilesObservatory filesObservatory,
                                                         string file)
      => filesObservatory.CreateObserver(GetDirectoryName(file),
                                         GetFileName(file),
                                         false);
  }
}