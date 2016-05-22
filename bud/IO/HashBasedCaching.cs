using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.IO.File;
using static Bud.Option;

namespace Bud.IO {
  public class HashBasedCaching {
    public static IEnumerable<string>
      GetLinesOrCache(string cacheFile,
                      string hash,
                      Func<IEnumerable<string>> contentLines) {
      var calculatedDigest = hash;
      var resultFromCache = TryGetFromCache(cacheFile, calculatedDigest);
      if (resultFromCache.HasValue) {
        return resultFromCache.Value;
      }
      var lines = contentLines();
      var linesAsList = lines as IList<string> ?? lines.ToList();
      WriteAllLines(cacheFile, new[] {calculatedDigest}.Concat(linesAsList));
      return linesAsList;
    }

    private static Option<IEnumerable<string>>
      TryGetFromCache(string cacheFile, string calculatedDigest) {
      var optCachedLines = None<IEnumerable<string>>();
      if (Exists(cacheFile)) {
        using (var stream = OpenRead(cacheFile)) {
          using (var reader = new StreamReader(stream)) {
            if (string.Equals(calculatedDigest, reader.ReadLine())) {
              var cachedLines = new List<string>();
              string currentLine;
              while ((currentLine = reader.ReadLine()) != null) {
                cachedLines.Add(currentLine);
              }
              optCachedLines = cachedLines;
            }
          }
        }
      }
      return optCachedLines;
    }
  }
}