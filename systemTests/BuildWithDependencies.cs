using Bud;
using System.Collections.Generic;
using Bud.Plugins;
using System.Collections.Immutable;

public class BuildWithDependencies : Build {

  public Settings GetSettings() {
    return Settings.Start
      .AddProject(id: "root", baseDir: ".");
  }

}


