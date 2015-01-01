using System.IO;
using System.Collections.Immutable;
using Bud.SettingsConstruction;

namespace Bud.Plugins.Build {

  public static class BuildDirs {
    public static string GetBaseDir(this IConfiguration buildConfiguration, Scope project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.BaseDir.In(project));
    }

    public static string GetBudDir(this IConfiguration buildConfiguration, Scope project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.BudDir.In(project));
    }

    public static string GetDefaultBudDir(this Configuration ctxt, Scope scope) {
      return Path.Combine(ctxt.GetBaseDir(scope), ".bud");
    }

    /// <returns>The directory where build output (such as compiled assemblies) are stored.</returns>
    /// <param name="projectBaseDir">The root directory of the project being built.</param>
    public static string GetOutputDir(this IConfiguration buildConfiguration, Scope project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.OutputDir.In(project));
    }

    public static string GetDefaultOutputDir(this IConfiguration ctxt, Scope scope) {
      return Path.Combine(ctxt.GetBudDir(scope), "output");
    }

    /// <returns>The directory where data gathered during build configuration is stored (e.g.: downloaded dependencies).</returns>
    /// <param name="projectBaseDir">The root directory of the project being built.</param>
    public static string GetBuildConfigCacheDir(this IConfiguration buildConfiguration, Scope project) {
      return buildConfiguration.Evaluate(BuildDirsKeys.BuildConfigCacheDir.In(project));
    }

    public static string GetDefaultBuildConfigCacheDir(this IConfiguration ctxt, Scope scope) {
      return Path.Combine(ctxt.GetBudDir(scope), "buildConfigCache");
    }
  }

}

