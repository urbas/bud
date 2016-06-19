using System.Collections.Immutable;

namespace Bud.Building {
  public delegate void FilesBuilder(ImmutableArray<string> inputFiles,
                                    string outputFile);
}