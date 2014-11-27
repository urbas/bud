using System;
using Bud.Plugin.CSharp;
using Bud.Plugins;
using System.Collections.Immutable;

namespace Bud {
  public static class Bud {

    public static BuildConfiguration Load(string path) {
      return new BuildConfiguration(path);
    }

    public static void Evaluate(BuildConfiguration buildConfiguration, string key) {
      if ("compile".Equals(key)) {
        CSharpPlugin.Compile(buildConfiguration);
      } else {
        ProjectPlugin.Clean(buildConfiguration);
      }
    }

  }
}

