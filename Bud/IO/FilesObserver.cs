using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Bud.IO {
  public class FilesObserver : IEnumerable<string> {
    public readonly static FilesObserver Empty = new FilesObserver(Enumerable.Empty<SourceDir>());

    public FilesObserver(IEnumerable<SourceDir> sourceDirs) {
      SourceDirs = sourceDirs;
    }

    public IEnumerable<SourceDir> SourceDirs { get; }

    public FilesObserver ExtendWith(string sourceDir, string fileFilter)
      => new FilesObserver(SourceDirs.Concat(new[] {new SourceDir(sourceDir, fileFilter)}));

    public IEnumerator<string> GetEnumerator()
      => SourceDirs.SelectMany(SourceDir.EnumerateFiles).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}