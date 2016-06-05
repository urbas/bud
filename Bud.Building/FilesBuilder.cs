using System.Collections.Generic;

namespace Bud.Building {
  public delegate void FilesBuilder(IReadOnlyList<string> inputFiles,
                                    string outputFile);
}