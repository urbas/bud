using System;
using System.IO;
using System.Threading.Tasks;

namespace Bud.Plugins.Build {

  public class BuildDirsPlugin : IPlugin {
    private readonly string baseDir;

    public BuildDirsPlugin(string baseDir) {
      this.baseDir = baseDir;
    }

    public Settings ApplyTo(Settings settings, Key scope) {
      return settings
        .Apply(scope, BuildPlugin.Instance)
        .Init(BuildDirsKeys.BaseDir.In(scope), baseDir)
        .Init(BuildDirsKeys.BudDir.In(scope), ctxt => BuildDirs.GetDefaultBudDir(ctxt, scope))
        .Init(BuildDirsKeys.OutputDir.In(scope), ctxt => BuildDirs.GetDefaultOutputDir(ctxt, scope))
        .Init(BuildDirsKeys.BuildConfigCacheDir.In(scope), ctxt => BuildDirs.GetDefaultBuildConfigCacheDir(ctxt, scope))
        .Init(BuildDirsKeys.PersistentBuildConfigDir.In(scope), ctxt => BuildDirs.GetDefaultPersistentBuildConfigDir(ctxt, scope))
        .Modify(BuildKeys.Clean.In(scope), (ctxt, oldTask) => CleanBuildDirsTask(ctxt, oldTask, scope));
    }

    private async static Task<Unit> CleanBuildDirsTask(EvaluationContext context, Func<Task<Unit>> oldCleanTask, Key project) {
      await oldCleanTask();
      Directory.Delete(context.GetOutputDir(project), true);
      return Unit.Instance;
    }
  }
}

