using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.CSharp;
using Bud.Plugins.Projects;

namespace Bud {
  public static class BuildConfigurationLoader {

    public static EvaluationContext Load(string path) {
      // Does the .bud/bakedBuild/Build.dll file exist?
      //  - load it and be done with it :)
      var defaultProjectSettings = Project.New("root", path).BuildsCSharp();
      return EvaluationContext.ToEvaluationContext(defaultProjectSettings);
    }

  }
}

