using System.Collections.Generic;
using System.IO;

namespace Bud.IO {
  public class SourceDir {
    public string Dir { get; }
    public string FileFilter { get; }

    public SourceDir(string dir, string fileFilter) {
      Dir = dir;
      FileFilter = fileFilter;
    }

    public static IEnumerable<string> EnumerateFiles(SourceDir dir)
      => Directory.EnumerateFiles(dir.Dir, dir.FileFilter, SearchOption.AllDirectories);
  }
}