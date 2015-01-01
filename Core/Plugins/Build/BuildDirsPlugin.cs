﻿using System;
using System.IO;

namespace Bud.Plugins.Build {

  public class BuildDirsPlugin : IPlugin {
    private readonly string baseDir;

    public BuildDirsPlugin(string baseDir) {
      this.baseDir = baseDir;
    }

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings
        .Apply(scope, BuildPlugin.Instance)
        .AddDependencies(BuildKeys.Clean, BuildKeys.Clean.In(scope))
        .Init(BuildDirsKeys.BaseDir.In(scope), baseDir)
        .Init(BuildDirsKeys.BudDir.In(scope), ctxt => BuildDirs.GetDefaultBudDir(ctxt, scope))
        .Init(BuildDirsKeys.OutputDir.In(scope), ctxt => BuildDirs.GetDefaultOutputDir(ctxt, scope))
        .Init(BuildDirsKeys.BuildConfigCacheDir.In(scope), ctxt => BuildDirs.GetDefaultBuildConfigCacheDir(ctxt, scope))
        .Init(BuildKeys.Clean.In(scope), ctxt => CleanBuildDirsTask(ctxt, scope));
    }

    private static Unit CleanBuildDirsTask(EvaluationContext context, Scope project) {
      var outputDir = context.GetOutputDir(project);
      Directory.Delete(outputDir, true);
      return Unit.Instance;
    }
  }
}

