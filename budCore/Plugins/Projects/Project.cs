using System.IO;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using Bud.SettingsConstruction.Ops;

namespace Bud.Plugins.Projects {

  public static class Project {
    public static Settings New(string id, string baseDir) {
      return Settings.Start.AddProject(id, baseDir);
    }

    public static Scope CreateProjectScope(this Settings settings, string id) {
      return new Scope(id).In(ProjectKeys.Project.In(settings.CurrentScope));
    }

    public static ImmutableDictionary<string, Scope> GetAllProjects(this EvaluationContext buildConfiguration) {
      return buildConfiguration.Evaluate(ProjectKeys.AllProjects);
    }

    public static Scope GetProject(this EvaluationContext context, string id) {
      return GetAllProjects(context)[id];
    }
  }

}

