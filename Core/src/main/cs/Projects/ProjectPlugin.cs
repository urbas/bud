using System.Collections.Immutable;
using Bud.Build;
using NuGet;

namespace Bud.Projects {
  public class ProjectPlugin : Plugin {
    private readonly string ProjectId;
    private readonly string BaseDir;
    private readonly Setup[] Setups;

    public ProjectPlugin(string projectId, string baseDir, params Setup[] setups) {
      ProjectId = projectId;
      BaseDir = baseDir;
      Setups = setups;
    }

    public override Settings Setup(Settings settings) {
      var project = ProjectKey(ProjectId);
      return settings
        .AddGlobally(ProjectKeys.Projects.Init(ImmutableDictionary<string, Key>.Empty),
                     ProjectKeys.Projects.Modify(allProjects => allProjects.Add(ProjectId, project)))
        .AddIn(project,
               new BuildDirsPlugin(BaseDir),
               ProjectKeys.Version.Init(VersionImpl),
               Setups.Merge());
    }

    public static Key ProjectKey(string projectId) {
      return Key.Root / ProjectKeys.Project / projectId;
    }

    private static SemanticVersion VersionImpl(IConfig config) {
      return config.IsConfigDefined(ProjectKeys.Version) ? config.Evaluate(ProjectKeys.Version) : SemanticVersion.Parse("0.0.1-SNAPSHOT");
    }
  }
}