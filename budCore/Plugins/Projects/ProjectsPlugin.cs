using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Bud.Plugins.Build;

namespace Bud.Plugins.Projects {

  public class ProjectPlugin : BudPlugin {
    private readonly string id;
    private readonly string baseDir;

    public ProjectPlugin(string id, string baseDir) {
      this.baseDir = baseDir;
      this.id = id;
    }

    public Settings ApplyTo(Settings settings, Scope scope) {
      var project = Project.Key(id, scope);
      return settings
        .InitOrKeep(ProjectKeys.Projects, ImmutableDictionary.Create<string, Scope>())
        .SetCurrentScope(project)
        .Add(new BuildDirsPlugin(baseDir))
        .Modify(ProjectKeys.Projects, allProjects => allProjects.Add(id, project));
    }
  }
}

