using System.Collections.Immutable;

namespace Bud.Build {
  public interface IBuilder {
    /// <summary>
    ///   This function must produce the file <paramref name="output"/>.
    /// </summary>
    /// <param name="output">the path at which this function must create a file.</param>
    /// <param name="inputFiles">the files from which this function can generate the output.</param>
    void Build(string output, IImmutableList<string> inputFiles);
  }
}