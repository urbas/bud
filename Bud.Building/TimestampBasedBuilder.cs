using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bud.Building {
  public class TimestampBasedBuilder {
    /// <summary>
    ///   Builds a single file with the given list of inputs.
    /// </summary>
    /// <param name="fileGenerator">this function actually produces the output.</param>
    /// <param name="inputFiles">the files from which the <paramref name="fileGenerator" /> should generate the output.</param>
    /// <param name="output">the path of the expected output.</param>
    /// <remarks>
    ///   The worker will not be invoked if the last-modified timestamp of the <paramref name="output" /> is newer
    ///   than the last-modified timestamp of any of the input files.
    /// </remarks>
    public static void Build(FilesBuilder fileGenerator,
                             IReadOnlyList<string> inputFiles,
                             string output) {
      inputFiles = inputFiles ?? BuildInput.EmptyInputFiles;
      if (!File.Exists(output) || AnyFileNewer(inputFiles, output)) {
        fileGenerator(inputFiles, output);
      }
    }

    private static bool AnyFileNewer(IEnumerable<string> files,
                                     string referenceFile) {
      var timestampOfReference = File.GetLastWriteTimeUtc(referenceFile);
      return files.Any(file => File.GetLastWriteTimeUtc(file) > timestampOfReference);
    }
  }
}