using System.Collections.Immutable;

namespace Bud.Building {
  public interface IFileGenerator {
    /// <summary>
    ///   This function must produce the file <paramref name="outputFile" />.
    /// </summary>
    /// <param name="outputFile">the path of the file this function should generate.</param>
    /// <param name="inputFiles">the files from which this function can generate the output.</param>
    void Generate(string outputFile, IImmutableList<string> inputFiles);
  }
}