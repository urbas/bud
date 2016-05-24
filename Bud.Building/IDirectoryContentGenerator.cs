using System.Collections.Immutable;

namespace Bud.Building {
  public interface IDirectoryContentGenerator {
    /// <summary>
    ///   This function should create files in the given <paramref name="outputDir" />.
    /// </summary>
    /// <param name="outputDir">the directory in which this function should produce files.</param>
    /// <param name="inputFiles">the input files this generator function can use to produce files.</param>
    /// <remarks>
    ///   this function should be deterministic, time-invariant, and output directory-invariant.
    ///   That is, it should produce the same output when given the same input regardless of when
    ///   it is called and what outpud directory it is given.
    /// </remarks>
    void Generate(string outputDir, IImmutableList<string> inputFiles);
  }
}