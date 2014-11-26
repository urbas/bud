using System.IO;

namespace Bud {
  public class DefaultBuildPlugin {
    public static void Clean(BuildConfiguration buildConfiguration) {
      Directory.Delete(BudPaths.GetOutputDirectory(buildConfiguration.ProjectBaseDir), true);
    }
  }
}

