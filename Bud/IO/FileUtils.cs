using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Bud.IO {
  public class FileUtils {
    public static Timestamped<string> ToTimestampedFile(string path)
      => new Timestamped<string>(path, GetFileTimestamp(path));

    public static long GetFileTimestamp(string path)
      => File.GetLastWriteTime(path).ToFileTime();

    public static long FileTimestampNow()
      => DateTime.Now.ToFileTime();

    public static IEnumerable<Timestamped<string>> ToTimestampedFiles(IEnumerable<string> sources)
      => sources.Select(ToTimestampedFile);

    public static bool IsNewerThan(string file, IEnumerable<string> referenceFiles) {
      var fileTimestamp = GetFileTimestamp(file);
      return referenceFiles.All(referenceFile => GetFileTimestamp(referenceFile) < fileTimestamp);
    }
  }
}