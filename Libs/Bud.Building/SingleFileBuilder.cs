namespace Bud.Building {
  /// <summary>
  ///   This builder takes a list of input files and produces an output file.
  ///   This method must be blocking. That is, by the time this method returns, the output
  ///   file must exist at the path specified in the <paramref name="outputFile" /> parameter.
  /// </summary>
  /// <param name="inputFile">input file path.</param>
  /// <param name="outputFile">the path to the output file this builder must produce.</param>
  public delegate void SingleFileBuilder(string inputFile, string outputFile);
}