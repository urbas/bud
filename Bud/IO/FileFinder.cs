using System.Collections.Generic;

namespace Bud.IO {
  public delegate IEnumerable<string> FileFinder(string sourceDir, string fileFilter, bool includeSubdirs);
}