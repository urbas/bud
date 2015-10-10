using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public static class FilesObservatory {
    public static readonly IObservable<IEnumerable<string>> Empty = Observable.Return(Enumerable.Empty<string>());

    public static IObservable<IEnumerable<string>> ObserveFiles(this IFilesObservatory filesObservatory, string sourceDir, string fileFilter, bool includeSubdirs)
      => ObserveFiles(filesObservatory, EnumerateFiles, sourceDir, fileFilter, includeSubdirs);

    public static IObservable<IEnumerable<string>> ObserveFiles(this IFilesObservatory filesObservatory, Func<string, string, bool, IEnumerable<string>> fileEnumerator, string sourceDir, string fileFilter, bool includeSubdirs)
      => Observable.Create<IEnumerable<string>>(observer => Observable.Return(fileEnumerator(sourceDir, fileFilter, includeSubdirs)).Subscribe(observer))
                   .Concat(filesObservatory.CreateObserver(sourceDir, fileFilter, includeSubdirs)
                                           .Select(args => fileEnumerator(sourceDir, fileFilter, includeSubdirs)));

    public static IObservable<IEnumerable<string>> ObserveFileList(this IFilesObservatory filesObservatory, IEnumerable<string> absolutePaths)
      => Observable.Create<IEnumerable<string>>(observer => Observable.Return(absolutePaths).Subscribe(observer))
                   .Concat(ObserveIndividualFiles(filesObservatory, absolutePaths).Select(args => absolutePaths));

    public static IObservable<IEnumerable<string>> ObserveFileList(this IFilesObservatory filesobservatory, params string[] absolutePaths)
      => ObserveFileList(filesobservatory, absolutePaths as IEnumerable<string>);

    public static IObservable<IEnumerable<string>> Join(IObservable<IEnumerable<string>> observedFiles, IObservable<IEnumerable<string>> otherObservedFiles)
      => observedFiles.CombineLatest(otherObservedFiles, (oldFiles, newFiles) => oldFiles.Concat(newFiles));

    public static IObservable<IEnumerable<string>> Filter(IObservable<IEnumerable<string>> observedFiles, Func<string, bool> predicate)
      => observedFiles.Select(enumerable => enumerable.Where(predicate));

    private static IEnumerable<string> EnumerateFiles(string sourceDir, string fileFilter, bool includeSubdirs)
      => Directory.EnumerateFiles(sourceDir, fileFilter, includeSubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

    private static IObservable<FileSystemEventArgs> ObserveIndividualFiles(IFilesObservatory filesObservatory, IEnumerable<string> absolutePaths)
      => absolutePaths.Select(file => filesObservatory.CreateObserver(Path.GetDirectoryName(file), Path.GetFileName(file), false)).Merge();
  }
}