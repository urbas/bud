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
        .In(project,
            BuildDirsKeys.Clean.Init(TaskUtils.NoOpTask),
            BuildDirsKeys.BaseDir.Init(baseDir),
            BuildDirsKeys.BudDir.Init(BuildDirs.GetDefaultBudDir),
            BuildDirsKeys.OutputDir.Init(BuildDirs.GetDefaultOutputDir),
            BuildDirsKeys.BuildConfigCacheDir.Init(BuildDirs.GetDefaultBuildConfigCacheDir),
            BuildDirsKeys.PersistentBuildConfigDir.Init(BuildDirs.GetDefaultPersistentBuildConfigDir),
            BuildDirsKeys.Clean.Modify(CleanBuildDirsTask)
        )
        .In(Key.Global,
            BuildDirsKeys.Clean.Init(TaskUtils.NoOpTask),
            BuildDirsKeys.Clean.DependsOn(BuildDirsKeys.Clean.In(project))
        );
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