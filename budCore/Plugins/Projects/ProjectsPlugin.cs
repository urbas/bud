using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Bud.Plugins.Build;

namespace Bud.Plugins.Projects {

  public static class ProjectPlugin {
    public static Settings AddProject(this Settings existingSettings, string id, string baseDir) {
      var project = existingSettings.CreateProjectScope(id);
      return existingSettings
        .AddBuildSupport()
        .InitOrKeep(ProjectKeys.AllProjects, ImmutableDictionary.Create<string, Scope>())
        .SetCurrentScope(project)
        .AddBuildDirs(baseDir)
        .Modify(ProjectKeys.AllProjects, listOfProjects => listOfProjects.Add(id, project));
    }
  }
}

