using System.Collections.Immutable;

namespace Bud.Building {
  public class DigestGenerator : IFileGenerator {
    public virtual void Generate(string outputFile, IImmutableList<string> inputFiles) {
      Digest.CreateDigestsJsonFile(outputFile, inputFiles);
    }
  }
}