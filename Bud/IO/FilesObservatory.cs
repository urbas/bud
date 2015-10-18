using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public static class FilesObservatory {
    public static Files ObserveDir(this IFilesObservatory filesObservatory,
                                   string sourceDir,
                                   string fileFilter,
                                   bool includeSubdirs)
      => new Files(FindFiles(sourceDir, fileFilter, includeSubdirs),
                   filesObservatory.CreateObserver(sourceDir, fileFilter, includeSubdirs));

    public static Files ObserveFiles(this IFilesObservatory filesObservatory, IEnumerable<string> absolutePaths)
      => new Files(absolutePaths,
                   WatchersForFiles(filesObservatory, absolutePaths));

    public static Files ObserveFiles(this IFilesObservatory filesobservatory, params string[] absolutePaths)
      => ObserveFiles(filesobservatory, absolutePaths as IEnumerable<string>);

    private static IEnumerable<string> FindFiles(string sourceDir, string fileFilter, bool includeSubdirs)
      => Directory.EnumerateFiles(sourceDir, fileFilter, ToSearchOption(includeSubdirs));

    private static SearchOption ToSearchOption(bool includeSubdirs)
      => includeSubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

    private static IObservable<FileSystemEventArgs> WatchersForFiles(IFilesObservatory filesObservatory, IEnumerable<string> absolutePaths)
      => absolutePaths.Select(file => SingleFileWatcher(filesObservatory, file))
                      .Merge();

    private static IObservable<FileSystemEventArgs> SingleFileWatcher(IFilesObservatory filesObservatory, string file)
      => filesObservatory.CreateObserver(Path.GetDirectoryName(file), Path.GetFileName(file), false);
  }
}