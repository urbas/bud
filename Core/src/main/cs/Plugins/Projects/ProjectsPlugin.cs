using System.Collections.Immutable;
using Bud.Plugins.Build;

namespace Bud.Plugins.Projects {
  public class ProjectPlugin : IPlugin {
    private readonly string id;
    private readonly string baseDir;

    public ProjectPlugin(string id, string baseDir) {
      this.baseDir = baseDir;
      this.id = id;
    }

    public Settings ApplyTo(Settings settings) {
      var project = settings.Scope;
      return settings
        .Apply(project, new BuildDirsPlugin(baseDir))
        .In(Key.Global,
            ProjectKeys.Projects.Init(ImmutableDictionary<string, Key>.Empty),
            ProjectKeys.Projects.Modify(allProjects => allProjects.Add(id, project))
        );
    }
  }
}