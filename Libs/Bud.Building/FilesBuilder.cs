using System.Collections.Immutable;

namespace Bud.Building {
  /// <summary>
  ///   This builder takes a list of input files and produces an output file.
  ///   This method must be blocking. That is, by the time this method returns, the output
  ///   file must exist at the path specified in the <paramref name="outputFile" /> parameter.
  /// </summary>
  /// <param name="inputFiles">input file paths.</param>
  /// <param name="outputFile">the path to the output file this builder must produce.</param>
  public delegate void FilesBuilder(ImmutableArray<string> inputFiles,
                                    string outputFile);
}