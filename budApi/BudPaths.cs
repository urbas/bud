using System;
using System.IO;

namespace Bud
{
	public static class BudPaths
  {

    public static string GetBuildDirectory(string projectBaseDir) {
      return Path.Combine(projectBaseDir, ".bud");
    }

    /// <returns>The directory where build output (such as compiled assemblies) are stored.</returns>
    /// <param name="projectBaseDir">The root directory of the project being built.</param>
    public static string GetOutputDirectory(string projectBaseDir) {
      return Path.Combine(GetBuildDirectory(projectBaseDir), "output");
    }

    /// <returns>The directory where data gathered during build configuration is stored (e.g.: downloaded dependencies).</returns>
    /// <param name="projectBaseDir">The root directory of the project being built.</param>
    public static string GetBuildCacheDirectory(string projectBaseDir) {
      return Path.Combine(GetBuildDirectory(projectBaseDir), "buildConfigCache");
    }

	}
}

