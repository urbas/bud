using System;
using System.IO;

namespace Bud
{
	public static class BudPaths
  {

    public static string GetBuildDirectory(string projectBaseDir) {
      return Path.Combine(projectBaseDir, ".bud");
    }

    public static string GetOutputDirectory(string projectBaseDir) {
      return Path.Combine(GetBuildDirectory(projectBaseDir), "output");
    }

	}
}

