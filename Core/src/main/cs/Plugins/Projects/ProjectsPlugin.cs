using System.Collections.Immutable;
using Bud.Plugins.Build;

namespace Bud.Plugins.Projects {
  public class ProjectPlugin {
    public static Setup Init(string projectId, string baseDir, Setup[] setups) {
      var projectKey = ProjectsSettings.ProjectKey(projectId);
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