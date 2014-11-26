using System;
using Bud.Plugin.CSharp;

namespace Bud {
  public static class Bud {

    public static BuildConfiguration Load(string path) {
      return new BuildConfiguration(path);
    }

    public static void Evaluate(BuildConfiguration buildConfiguration, string key) {
      CSharpPlugin.Compile(buildConfiguration);
    }

  }
}

