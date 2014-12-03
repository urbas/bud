using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.CSharp;
using Bud.Plugins.Projects;

namespace Bud {
  public static class BuildLoader {

    public static EvaluationContext Load(string path) {
      // Does the .bud/bakedBuild/Build.dll file exist?
      //  - load it and be done with it :)
      return EvaluationContext.ToEvaluationContext(CSharp.Project("root", path));
    }

  }
}

