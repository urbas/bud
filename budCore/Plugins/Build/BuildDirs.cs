using System.IO;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using Bud.SettingsConstruction.Ops;

namespace Bud.Plugins.Build {

  public static class BuildDirs {
    public static string GetBaseDir(this EvaluationContext buildConfiguration, Scope project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.BaseDir.In(project));
    }

    public static string GetBudDir(this EvaluationContext buildConfiguration, Scope project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.BudDir.In(project));
    }

    public static string GetDefaultBudDir(this EvaluationContext ctxt, Scope scope) {
      return Path.Combine(ctxt.GetBaseDir(scope), ".bud");
    }

    /// <returns>The directory where build output (such as compiled assemblies) are stored.</returns>
    /// <param name="projectBaseDir">The root directory of the project being built.</param>
    public static string GetOutputDir(this EvaluationContext buildConfiguration, Scope project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.OutputDir.In(project));
    }

    public static string GetDefaultOutputDir(this EvaluationContext ctxt, Scope scope) {
      return Path.Combine(ctxt.GetBudDir(scope), "output");
    }

    /// <returns>The directory where data gathered during build configuration is stored (e.g.: downloaded dependencies).</returns>
    /// <param name="projectBaseDir">The root directory of the project being built.</param>
    public static string GetBuildConfigCacheDir(this EvaluationContext buildConfiguration, Scope project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.BuildConfigCacheDir.In(project));
    }

    public static string GetDefaultBuildConfigCacheDir(this EvaluationContext ctxt, Scope scope) {
      return Path.Combine(ctxt.GetBudDir(scope), "buildConfigCache");
    }
  }

}

