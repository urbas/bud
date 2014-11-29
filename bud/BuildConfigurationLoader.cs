using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.CSharp;

namespace Bud {
  public static class BuildConfigurationLoader {

    public static BuildConfiguration Load(string path) {
      // Does the .bud/bakedBuild/Build.dll file exist?
      //  - load it and be done with it :)
      var defaultProjectSettings = ProjectPlugin.Project(path, path).AddCSharpSupport();
      return BuildConfiguration.ToBuildConfiguration(defaultProjectSettings);
    }

  }
}

