using System;
using Bud.Plugin.CSharp;
using Bud.Plugins;

namespace Bud {
  public static class Bud {

    public static BuildConfiguration Load(string path) {
      return new BuildConfiguration(path);
    }

    public static void Evaluate(BuildConfiguration buildConfiguration, string key) {
      if ("compile".Equals(key)) {
        CSharpPlugin.Compile(buildConfiguration);
      } else {
        BuildPlugin.Clean(buildConfiguration);
      }
    }

  }
}

