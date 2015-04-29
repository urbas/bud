using System;
using System.IO;
using System.Threading.Tasks;
using Bud.BuildDefinition;
using Bud.Util;

namespace Bud.Build {
  public class BuildDirsPlugin : Plugin {
    public const string OutputDirName = "output";
    public const string BuildConfigCacheDirName = "buildConfigCache";
    public const string PersistentBuildConfigDirName = "persistentBuildConfig";
    public readonly string BaseDir;

    public BuildDirsPlugin(string baseDir) {
      BaseDir = baseDir;
    }

    public override Settings Setup(Settings settings) {
      return settings
        .Add(BuildDirsKeys.Clean.Init(TaskUtils.NoOpTask),
             BuildDirsKeys.BaseDir.Init(DefaultBaseDir),
             BuildDirsKeys.BudDir.Init(GetDefaultBudDir),
             BuildDirsKeys.OutputDir.Init(GetDefaultOutputDir),
             BuildDirsKeys.BuildConfigCacheDir.Init(GetDefaultBuildConfigCacheDir),
             BuildDirsKeys.PersistentBuildConfigDir.Init(GetDefaultPersistentBuildConfigDir),
             BuildDirsKeys.Clean.Modify(CleanBuildDirsImpl))
        .AddGlobally(BuildDirsKeys.Clean.Init(TaskUtils.NoOpTask),
                     BuildDirsKeys.Clean.DependsOn(settings.Scope / BuildDirsKeys.Clean));
    }

    private string DefaultBaseDir(IConfig config, Key scopeKey) {
      return string.IsNullOrEmpty(BaseDir) ? Path.Combine(config.GetBaseDir(), scopeKey.Id) : BaseDir;
    }

    public static string GetDefaultBudDir(IConfig ctxt, Key key) {
      return Path.Combine(ctxt.GetBaseDir(key), BudPaths.BudDirName);
    }

    public static string GetDefaultOutputDir(IConfig ctxt, Key key) {
      return Path.Combine(ctxt.GetBudDir(key), OutputDirName);
    }

    public static string GetDefaultBuildConfigCacheDir(IConfig ctxt, Key key) {
      return Path.Combine(ctxt.GetBudDir(key), BuildConfigCacheDirName);
    }

    public static string GetDefaultPersistentBuildConfigDir(IConfig ctxt, Key key) {
      return Path.Combine(ctxt.GetBudDir(key), PersistentBuildConfigDirName);
    }

    private static async Task CleanBuildDirsImpl(IContext context, Func<Task> oldCleanTask, Key project) {
      await oldCleanTask();
      var dir = context.GetOutputDir(project);
      if (Directory.Exists(dir)) {
        Directory.Delete(dir, true);
      }
    }
  }
}