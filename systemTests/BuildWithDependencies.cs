using Bud;
using System.Collections.Generic;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Settings;

public class BuildWithDependencies : Build {

  public ImmutableList<Setting> Settings() {
    return BuildConfiguration.Start
      .AddProject(id: "root", baseDir: ".");
  }

}


