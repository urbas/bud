using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using static System.IO.Directory;
using static System.IO.SearchOption;

namespace Bud.IO {
  public class FilesInDir : IFiles {
    public SearchOption SearchOption { get; }
    public IFilesObservatory FilesObservatory { get; }
    public string SourceDir { get; }
    public string FileFilter { get; }

    public FilesInDir(IFilesObservatory filesObservatory, string sourceDir, string fileFilter, SearchOption searchOption = AllDirectories) {
      FilesObservatory = filesObservatory;
      SourceDir = sourceDir;
      FileFilter = fileFilter;
      SearchOption = searchOption;
    }

    public IEnumerable<string> Enumerate() => EnumerateFiles(SourceDir, FileFilter, SearchOption);

    public IObservable<FilesUpdate> Watch()
      => Observable.Return(ToFilesUpdate(null))
                   .Concat(CreateFilesObserver().Select(ToFilesUpdate));

    private FilesUpdate ToFilesUpdate(FileSystemEventArgs fileSystemEventArgs)
      => new FilesUpdate(fileSystemEventArgs, this);

    private IObservable<FileSystemEventArgs> CreateFilesObserver()
      => FilesObservatory.CreateObserver(SourceDir, FileFilter, SearchOption == AllDirectories);
  }
}