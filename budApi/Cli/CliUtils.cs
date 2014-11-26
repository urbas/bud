using System;

namespace Bud {
  public static class CliUtils {
    public static CliArguments Argument(string cSharpCompiler) {
      return new CliArguments().Add(cSharpCompiler);
    }
  }
}

