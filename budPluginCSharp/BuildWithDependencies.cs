using Bud;
using System.Collections.Generic;
using Bud.Plugins;
using System.Collections.Immutable;

public class BuildWithDependencies : Build {

  public ImmutableList<Setting> Settings() {

    var rootProject = ProjectPlugin.CreateProject(id: "root", baseDir: ".");

    return rootProject;
  }

}


