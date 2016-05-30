using System.Collections.Generic;

namespace Bud.Building {
  public delegate void FilesBuilder(IEnumerable<string> inputFiles, string outputFile);
}