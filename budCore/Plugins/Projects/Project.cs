using System.IO;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using Bud.SettingsConstruction.Ops;

namespace Bud.Plugins.Projects {

  public static class Project {
    public static ScopedSettings New(string id, string baseDir) {
      return Settings.Start.AddProject(id, baseDir);
    }

    public static Scope Key(string id) {
      return new Scope(id).In(ProjectKeys.Projects);
    }

    public static ImmutableHashSet<Scope> GetListOfProjects(this EvaluationContext buildConfiguration) {
      return buildConfiguration.Evaluate(ProjectKeys.ListOfProjects);
    }

    public static string GetBaseDir(this EvaluationContext buildConfiguration, Scope project) {
      return buildConfiguration.Evaluate(ProjectKeys.BaseDir.In(project));
    }

    public static string GetBudDir(this EvaluationContext buildConfiguration, Scope project) {
      return buildConfiguration.Evaluate(ProjectKeys.BudDir.In(project));
    }

    public static string GetDefaultBudDir(this EvaluationContext ctxt, Scope scope) {
      return Path.Combine(ctxt.GetBaseDir(scope), ".bud");
    }

    /// <returns>The directory where build output (such as compiled assemblies) are stored.</returns>
    /// <param name="projectBaseDir">The root directory of the project being built.</param>
    public static string GetOutputDir(this EvaluationContext buildConfiguration, Scope project) {
      return buildConfiguration.Evaluate(ProjectKeys.OutputDir.In(project));
    }

    public static string GetDefaultOutputDir(this EvaluationContext ctxt, Scope scope) {
      return Path.Combine(ctxt.GetBudDir(scope), "output");
    }

    /// <returns>The directory where data gathered during build configuration is stored (e.g.: downloaded dependencies).</returns>
    /// <param name="projectBaseDir">The root directory of the project being built.</param>
    public static string GetBuildConfigCacheDir(this EvaluationContext buildConfiguration, Scope project) {
      return buildConfiguration.Evaluate(ProjectKeys.BuildConfigCacheDir.In(project));
    }

    public static string GetDefaultBuildConfigCacheDir(this EvaluationContext ctxt, Scope scope) {
      return Path.Combine(ctxt.GetBudDir(scope), "buildConfigCache");
    }
  }

}

