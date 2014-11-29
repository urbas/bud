using Bud;
using System.Collections.Generic;
using Bud.Plugins;
using System.Collections.Immutable;

public class BuildWithDependencies : Build {

  public Settings GetSettings(string baseDir) {
    return ProjectPlugin.Project("root", baseDir);
  }

}


