using System;

namespace Bud {
  public static class Bud {

    public static BuildConfiguration Load(string path) {
      return new BuildConfiguration(path);
    }

    public static void Execute(BuildConfiguration buildConfiguration, string taskName) {

    }

  }
}

