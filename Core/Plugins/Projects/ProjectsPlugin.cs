using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Bud.Plugins.Build;

namespace Bud.Plugins.Projects {

  public class ProjectPlugin : IPlugin {
    private readonly string id;
    private readonly string baseDir;

    public ProjectPlugin(string id, string baseDir) {
      this.baseDir = baseDir;
      this.id = id;
    }

    public Settings ApplyTo(Settings settings, Scope projectScope) {
      return settings
        .InitOrKeep(ProjectKeys.Projects, ImmutableDictionary.Create<string, Scope>())
        .Apply(projectScope, new BuildDirsPlugin(baseDir))
        .Modify(ProjectKeys.Projects, allProjects => allProjects.Add(id, projectScope));
    }
  }
}

