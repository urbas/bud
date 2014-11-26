using Bud;
using System.Collections.Generic;
using Bud.Plugins;
using System.Collections.Immutable;

public class BuildWithDependencies : Build {

  public ImmutableList<Setting> Settings() {

    var rootProject = BuildPlugin.AddProject(baseDir: ".");
//      .Using<CSharpPlugin>()
//      .WithDependency("Foo.Bar", "1.2.3");

    return rootProject;
  }

}


