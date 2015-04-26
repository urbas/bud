using System.Collections.Generic;

namespace Bud.Build {
  public static class BuildTargetKeys {
    public static readonly TaskKey<IEnumerable<string>> SourceFiles = Key.Define("sourceFiles");
  }
}