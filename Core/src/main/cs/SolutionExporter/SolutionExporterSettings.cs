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
      var allProjects = context.GetAllProjects()
                               .Select(idToProject => idToProject.Value);
      var mainBuildTargets = allProjects.Where(project => context.HasBuildTarget(project, BuildKeys.Main, CSharpKeys.CSharp))
                                        .Select(project => project / BuildKeys.Main / CSharpKeys.CSharp);
      foreach (var buildTarget in mainBuildTargets) {
        var buildTargetCsprojFile = GetCsprojFileFor(context, buildTarget);
        context.Logger.Info(context.LogMessage(buildTarget, "Generating '{0}'...", buildTargetCsprojFile));
      }
      return TaskUtils.NullAsyncResult;
    }

    private static string GetCsprojFileFor(IContext context, Key buildTarget) {
      return Path.Combine(context.GetBaseDir(buildTarget),
                          context.GetCSharpOutputAssemblyName(buildTarget) + ".csproj");
    }
  }
}