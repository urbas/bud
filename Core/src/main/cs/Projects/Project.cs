using System.Collections.Immutable;
using Bud.Build;
using NuGet;

namespace Bud.Projects {
  public class Project : Plugin {
    private readonly string ProjectId;
    private readonly string BaseDir;
    private readonly Setup[] Setups;

    public Project(string projectId, string baseDir, params Setup[] setups) {
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
      // TODO: Read the version from the file `.bud/version` or `version` or maybe even `Your.Project.project.json`
      return config.IsConfigDefined(ProjectKeys.Version) ? config.Evaluate(ProjectKeys.Version) : SemanticVersion.Parse("0.0.1-SNAPSHOT");
    }
  }
}