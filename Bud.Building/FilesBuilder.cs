using System.Collections.Immutable;

namespace Bud.Building {
  public delegate void FilesBuilder(IImmutableList<string> inputFiles,
                                    string outputFile);
}