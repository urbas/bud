using System.IO;
using Bud;
using Bud.Build;
using Bud.BuildDefinition;

static internal class DeploymentUtils {
  public static string GetDeploymentTemplatesDir(this IConfig config) {
    return Path.Combine(config.GetBaseDir(), BudPaths.BudDirName, "DeploymentTemplates");
  }
}