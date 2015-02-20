using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Build;
using NuGet;

namespace Bud.Projects {
  public static class ProjectsSettings {
    public static Settings Project(this Settings settings, string id, string baseDir, params Setup[] setups) {
      return settings.Do(Init(id, baseDir, setups));
    }

    public static Settings Version(this Settings settings, string version) {
      return settings.Do(ProjectKeys.Version.Modify(SemanticVersion.Parse(version)));
    }

    public static Key ProjectKey(string projectId) {
      return ProjectKeys.Project / projectId;
    }

    private static Setup Init(string projectId, string baseDir, IEnumerable<Setup> setups) {
      var project = ProjectKey(projectId);
      return settings => settings
        .Globally(ProjectKeys.Projects.Init(ImmutableDictionary<string, Key>.Empty),
                  ProjectKeys.Projects.Modify(allProjects => allProjects.Add(projectId, project)))
        .In(project,
            BuildDirs.Init(baseDir),
            ProjectKeys.Version.Init(VersionImpl),
            setups.ToPlugin());
    }

    private static SemanticVersion VersionImpl(IConfig config) {
      if (config.IsConfigDefined(ProjectKeys.Version)) {
        return config.Evaluate(ProjectKeys.Version);
      }
      return SemanticVersion.Parse("0.0.1");
    }
  }
}