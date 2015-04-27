using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Bud.Util;

namespace Bud.Build {
  public abstract class BuildTargetPlugin : Plugin {
    protected readonly Key BuildScope;
    protected readonly Key Language;
    private readonly Setup ExtraBuildTargetSetup;

    protected BuildTargetPlugin(Key buildScope, Key language, IEnumerable<Setup> extraBuildTargetSetup) {
      BuildScope = buildScope;
      Language = language;
      ExtraBuildTargetSetup = (extraBuildTargetSetup ?? ImmutableList<Setup>.Empty).Merge();
    }

    public override Settings Setup(Settings projectSettings) {
      var project = projectSettings.Scope;
      var buildTargetKey = project / BuildScope / Language;
      var globalBuildScopeTask = BuildScope / BuildKeys.Build;
      return projectSettings
        .Add(BuildKeys.BuildTargets.Init(ImmutableList<Key>.Empty),
             BuildKeys.BuildTargets.Modify((ctxt, oldBuildTargets) => oldBuildTargets.Add(buildTargetKey)))
        .AddIn(buildTargetKey,
               BuildDirsKeys.BaseDir.Init(context => Path.Combine(context.GetBaseDir(project), "src", BuildScope.Id, Language.Id)),
               BuildDirsKeys.OutputDir.Init(context => Path.Combine(context.GetOutputDir(project), BuildScope.Id, Language.Id)),
               BuildKeys.Build.Init(BuildTaskImpl),
               BuildKeys.Test.Init(TaskUtils.NoOpTask),
               BuildTargetKeys.PackageId.Init(BuildTargetUtils.DefaultPackageId(project, BuildScope)),
               BuildTargetSetup,
               ExtraBuildTargetSetup)
        .AddGlobally(BuildKeys.Test.Init(TaskUtils.NoOpTask),
                     BuildKeys.Test.DependsOn(buildTargetKey / BuildKeys.Test),
                     BuildKeys.Build.Init(TaskUtils.NoOpTask),
                     BuildKeys.Build.DependsOn(globalBuildScopeTask),
                     globalBuildScopeTask.Init(TaskUtils.NoOpTask),
                     globalBuildScopeTask.DependsOn(buildTargetKey / BuildKeys.Build));
    }

    protected abstract Settings BuildTargetSetup(Settings projectSettings);

    private static Task BuildTaskImpl(IContext context, Key buildTarget) => TaskUtils.NullAsyncResult;
  }
}