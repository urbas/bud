using System.Collections.Immutable;

namespace Bud.Build {
  public class DigestGenerator : IOutputGenerator {
    public virtual void Generate(string output, IImmutableList<string> inputFiles) {
      Digest.CreateDigestsJsonFile(output, inputFiles);
    }
  }
}