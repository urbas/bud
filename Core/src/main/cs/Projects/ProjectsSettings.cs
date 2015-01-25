using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Build;

namespace Bud.Projects {
  public static class ProjectsSettings {
    public static Settings Project(this Settings settings, string id, string baseDir, params Setup[] setups) {
      return settings.Do(Init(id, baseDir, setups));
    }

    public static Key ProjectKey(string projectId) {
      return new Key(projectId).In(ProjectKeys.Project);
    }

    private static Setup Init(string projectId, string baseDir, IEnumerable<Setup> setups) {
      var projectKey = ProjectKey(projectId);
      return settings => settings
        .In(projectKey,
            BuildDirs.Init(baseDir),
            setups.ToPlugin())
        .In(Key.Global,
            ProjectKeys.Projects.Init(ImmutableDictionary<string, Key>.Empty),
            ProjectKeys.Projects.Modify(allProjects => allProjects.Add(projectId, projectKey)));
    }
  }
}