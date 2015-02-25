using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Build;
using Bud.CSharp;
using Bud.Projects;
using Bud.Util;

namespace Bud.SolutionExporter {
  public static class SolutionExporterSettings {
    public static Settings ExportAsSolution(this Settings settings) {
      return settings.Globally(SolutionExporterKeys.ExportAsSolution.Init(ExportAsSolutionImpl));
    }

    private static Task ExportAsSolutionImpl(IContext context) {
      foreach (var buildTarget in BuildTargetUtils.GetAllBuildTargets(context)) {
        var buildTargetCsprojFile = GetBuildTargetCsprojPath(context, buildTarget);
        context.Logger.Info(string.Format("Generating '{0}'...", buildTargetCsprojFile));
      }
      return TaskUtils.NullAsyncResult;
    }

    private static string GetBuildTargetCsprojPath(IContext context, Key buildTarget) {
      return Path.Combine(context.GetBaseDir(buildTarget),
                          context.GetCSharpOutputAssemblyName(buildTarget) + ".csproj");
    }
  }
}