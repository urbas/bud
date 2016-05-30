using System.Collections.Generic;

namespace Bud.Building {
  public delegate void DirFromFilesBuilder(IEnumerable<string> inputFiles, string outputDir);
}