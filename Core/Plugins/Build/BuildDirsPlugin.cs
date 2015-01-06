using System;
using System.IO;
using System.Threading.Tasks;

namespace Bud.Plugins.Build {

  public class BuildDirsPlugin : IPlugin {
    private readonly string baseDir;

    public BuildDirsPlugin(string baseDir) {
      this.baseDir = baseDir;
    }

    public Settings ApplyTo(Settings settings, Key key) {
      return settings
        .Apply(key, BuildPlugin.Instance)
        .Init(BuildDirsKeys.BaseDir.In(key), baseDir)
        .Init(BuildDirsKeys.BudDir.In(key), ctxt => BuildDirs.GetDefaultBudDir(ctxt, key))
        .Init(BuildDirsKeys.OutputDir.In(key), ctxt => BuildDirs.GetDefaultOutputDir(ctxt, key))
        .Init(BuildDirsKeys.BuildConfigCacheDir.In(key), ctxt => BuildDirs.GetDefaultBuildConfigCacheDir(ctxt, key))
        .Init(BuildDirsKeys.PersistentBuildConfigDir.In(key), ctxt => BuildDirs.GetDefaultPersistentBuildConfigDir(ctxt, key))
        .Modify(BuildKeys.Clean.In(key), (ctxt, oldTask) => CleanBuildDirsTask(ctxt, oldTask, key));
    }

    private async static Task<Unit> CleanBuildDirsTask(IContext context, Func<Task<Unit>> oldCleanTask, Key project) {
      await oldCleanTask();
      var dir = context.GetOutputDir(project);
      if (Directory.Exists(dir)) {
        Directory.Delete(dir, true);
      }
      return Unit.Instance;
    }
  }
}

