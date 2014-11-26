using System;

namespace Bud.Cli {
  public static class Processes {
    public static ProcessBuilder Execute(string executablePath) {
      return new ProcessBuilder(executablePath);
    }
  }
}

