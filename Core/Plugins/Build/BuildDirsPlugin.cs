using System;
using System.IO;
using System.Threading.Tasks;
using Bud.Util;

namespace Bud.Plugins.Build {
  public class BuildDirsPlugin : IPlugin {
    private readonly string baseDir;

    public BuildDirsPlugin(string baseDir) {
      this.baseDir = baseDir;
    }

    public Settings ApplyTo(Settings settings, Key project) {
      return settings
        .Init(BuildDirsKeys.Clean, TaskUtils.NoOpTask)
        .Init(BuildDirsKeys.Clean.In(project), TaskUtils.NoOpTask)
        .Init(BuildDirsKeys.BaseDir.In(project), baseDir)
        .Init(BuildDirsKeys.BudDir.In(project), ctxt => BuildDirs.GetDefaultBudDir(ctxt, project))
        .Init(BuildDirsKeys.OutputDir.In(project), ctxt => BuildDirs.GetDefaultOutputDir(ctxt, project))
        .Init(BuildDirsKeys.BuildConfigCacheDir.In(project), ctxt => BuildDirs.GetDefaultBuildConfigCacheDir(ctxt, project))
        .Init(BuildDirsKeys.PersistentBuildConfigDir.In(project), ctxt => BuildDirs.GetDefaultPersistentBuildConfigDir(ctxt, project))
        .AddDependencies(BuildDirsKeys.Clean, BuildDirsKeys.Clean.In(project))
        .Modify(BuildDirsKeys.Clean.In(project), (ctxt, oldTask) => CleanBuildDirsTask(ctxt, oldTask, project));
    }

    private static async Task<Unit> CleanBuildDirsTask(IContext context, Func<Task<Unit>> oldCleanTask, Key project) {
      await oldCleanTask();
      var dir = context.GetOutputDir(project);
      if (Directory.Exists(dir)) {
        Directory.Delete(dir, true);
      }
      return Unit.Instance;
    }
  }
}