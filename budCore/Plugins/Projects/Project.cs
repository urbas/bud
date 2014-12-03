using System.IO;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using Bud.SettingsConstruction.Ops;

namespace Bud.Plugins.Projects {

  public static class Project {
    public static Settings New(string id, string baseDir) {
      return Settings.Start.Add(new ProjectPlugin(id, baseDir));
    }

    public static Scope Key(string id, Scope scope) {
      return new Scope(id).In(ProjectKeys.Project.In(scope));
    }

    public static ImmutableDictionary<string, Scope> GetAllProjects(this EvaluationContext buildConfiguration) {
      return buildConfiguration.Evaluate(ProjectKeys.Projects);
    }

    public static Scope GetProject(this EvaluationContext context, string id) {
      return GetAllProjects(context)[id];
    }
  }

}

