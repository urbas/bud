using System.Collections.Immutable;
using Bud.Build;
using Bud.BuildDefinition;
using Bud.Commander;
using Bud.CSharp;
using NuGet;

namespace Bud.Projects {
  public static class ProjectPlugin {
    public static Setup Project(string projectId, string baseDir, params Setup[] setups) {
      var project = ProjectKey(projectId);
      return settings => settings
        .Globally(ProjectKeys.Projects.Init(ImmutableDictionary<string, Key>.Empty),
                  ProjectKeys.Projects.Modify(allProjects => allProjects.Add(projectId, project)))
        .In(project,
            BuildDirs.Init(baseDir),
            ProjectKeys.Version.Init(VersionImpl),
            setups.ToSetup());
    }

    public static Setup BudPlugin(string projectId, string baseDir, Setup[] setups) {
      return Project(projectId, baseDir, Cs.Dll(BuildDefinitionPlugin.BudAssemblyReferences, setups.ToSetup()));
    }

    public static Key ProjectKey(string projectId) {
      return Key.Root / ProjectKeys.Project / projectId;
    }

    private static SemanticVersion VersionImpl(IConfig config) {
      return config.IsConfigDefined(ProjectKeys.Version) ? config.Evaluate(ProjectKeys.Version) : SemanticVersion.Parse("0.0.1-SNAPSHOT");
    }
  }
}