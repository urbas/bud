using System;
using System.Collections.Immutable;
using Bud.Plugins.Build;

namespace Bud.Plugins.Projects {
  public class ProjectPlugin {
    public static Func<Settings, Settings> Init(string projectId, string baseDir, Func<Settings, Settings>[] plugins) {
      var projectKey = ProjectsSettings.ProjectKey(projectId);
      return settings => settings
        .In(projectKey,
            BuildDirsPlugin.Init(baseDir),
            plugins.ToSettingsTransform())
        .In(Key.Global,
            ProjectKeys.Projects.Init(ImmutableDictionary<string, Key>.Empty),
            ProjectKeys.Projects.Modify(allProjects => allProjects.Add(projectId, projectKey)));
    }
  }
}