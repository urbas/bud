using System.Collections.Generic;

namespace Bud.IO {
  public delegate IEnumerable<string> FileEnumerator(string sourceDir, string fileFilter, bool includeSubdirs);
}