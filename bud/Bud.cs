using System;
using Bud.Plugin.CSharp;
using Bud.Plugins;
using System.Collections.Immutable;

namespace Bud {
  public static class Bud {

    public static BuildConfiguration Load(string path) {
      throw new NotImplementedException();
    }

    public static void Evaluate(BuildConfiguration buildConfiguration, string key) {
      throw new NotImplementedException();
//      if ("compile".Equals(key)) {
//        CSharpPlugin.Compile(buildConfiguration);
//      } else {
//        ProjectPlugin.Clean(buildConfiguration);
//      }
    }

  }
}

