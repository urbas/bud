using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Build;
using NuGet;

namespace Bud.Projects {
  public static class ProjectsSettings {
    public static Settings Project(this Settings settings, string id, string baseDir, params Setup[] setups) {
      return settings.Do(Init(id, baseDir, setups));
    }

    public static Setup Version(string version) {
      return Settings.Modify(ProjectKeys.Version.Init(SemanticVersion.Parse(version)));
    }

    public static Key ProjectKey(string projectId) {
      return new Key(projectId).In(ProjectKeys.Project);
    }

    private static Setup Init(string projectId, string baseDir, IEnumerable<Setup> setups) {
      var project = ProjectKey(projectId);
      return settings => settings
        .Globally(ProjectKeys.Projects.Init(ImmutableDictionary<string, Key>.Empty),
                  ProjectKeys.Projects.Modify(allProjects => allProjects.Add(projectId, project)))
        .In(project,
            BuildDirs.Init(baseDir),
            ProjectKeys.Version.Init(SemanticVersion.Parse("0.0.1")),
            setups.ToPlugin());
    }
  }
}