using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public static class FilesObservatory {
    public static IObservable<Files> ObserveDir(this IFilesObservatory filesObservatory,
                                                string sourceDir,
                                                string fileFilter,
                                                bool includeSubdirs)
      => ObserveDir(filesObservatory, FindFiles, sourceDir, fileFilter, includeSubdirs);

    public static IObservable<Files> ObserveDir(this IFilesObservatory filesObservatory,
                                                FileFinder fileFinder,
                                                string sourceDir,
                                                string fileFilter,
                                                bool includeSubdirs)
      => SingletonFilesStream(fileFinder, sourceDir, fileFilter, includeSubdirs)
        .Concat(WatchingFileListStream(filesObservatory, fileFinder, sourceDir, fileFilter, includeSubdirs));

    public static IObservable<Files> ObserveFiles(this IFilesObservatory filesObservatory, IEnumerable<string> absolutePaths)
      => SingletonFilesStream(new Files(absolutePaths))
        .Concat(WatchersForFiles(filesObservatory, absolutePaths)
                  .Select(_ => new Files(absolutePaths)));

    public static IObservable<Files> ObserveFiles(this IFilesObservatory filesobservatory, params string[] absolutePaths)
      => ObserveFiles(filesobservatory, absolutePaths as IEnumerable<string>);

    private static IObservable<Files> SingletonFilesStream(Files files)
      => Observable.Create<Files>(observer => Observable.Return(files).Subscribe(observer));

    private static Files FindFiles(string sourceDir, string fileFilter, bool includeSubdirs)
      => new Files(Directory.EnumerateFiles(sourceDir, fileFilter, ToSearchOption(includeSubdirs)));

    private static IObservable<Files> WatchingFileListStream(IFilesObservatory filesObservatory, FileFinder fileFinder, string sourceDir, string fileFilter, bool includeSubdirs)
      => filesObservatory.CreateObserver(sourceDir, fileFilter, includeSubdirs)
                         .Select(args => fileFinder(sourceDir, fileFilter, includeSubdirs));

    private static SearchOption ToSearchOption(bool includeSubdirs)
      => includeSubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

    private static IObservable<FileSystemEventArgs> WatchersForFiles(IFilesObservatory filesObservatory, IEnumerable<string> absolutePaths)
      => absolutePaths.Select(file => SingleFileWatcher(filesObservatory, file))
                      .Merge();

    private static IObservable<FileSystemEventArgs> SingleFileWatcher(IFilesObservatory filesObservatory, string file)
      => filesObservatory.CreateObserver(Path.GetDirectoryName(file), Path.GetFileName(file), false);

    private static IObservable<Files> SingletonFilesStream(FileFinder fileFinder, string sourceDir, string fileFilter, bool includeSubdirs) {
      return Observable.Create<Files>(observer =>
                                        Observable.Return(fileFinder(sourceDir, fileFilter, includeSubdirs))
                                                  .Subscribe(observer));
    }
  }
}