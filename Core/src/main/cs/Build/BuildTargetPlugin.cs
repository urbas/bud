using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Bud.Util;

namespace Bud.Build {
  public abstract class BuildTargetPlugin {
    protected readonly Key BuildScope;
    protected readonly Key Language;
    private readonly IEnumerable<Setup> setups;

    protected BuildTargetPlugin(Key buildScope, Key language, IEnumerable<Setup> setups) {
      BuildScope = buildScope;
      Language = language;
      this.setups = setups ?? ImmutableList<Setup>.Empty;
    }

    public Settings Setup(Settings projectSettings) {
      var project = projectSettings.Scope;
      var buildTargetKey = project / BuildScope / Language;
      return projectSettings
        .Do(BuildKeys.Test.Init(TaskUtils.NoOpTask),
            BuildKeys.BuildTargets.Init(ImmutableList<Key>.Empty),
            BuildKeys.BuildTargets.Modify((ctxt, oldBuildTargets) => oldBuildTargets.Add(buildTargetKey)))
        .In(buildTargetKey,
            BuildDirsKeys.BaseDir.Init(context => Path.Combine(context.GetBaseDir(project), "src", BuildScope.Id, Language.Id)),
            BuildDirsKeys.OutputDir.Init(context => Path.Combine(context.GetOutputDir(project), BuildScope.Id, Language.Id)),
            BuildKeys.Build.Init(BuildTaskImpl),
            buildTargetSettings => Setup(buildTargetSettings, project),
            setups.ToSetup())
        .Globally(BuildKeys.Test.Init(TaskUtils.NoOpTask),
                  BuildKeys.Test.DependsOn(project / BuildKeys.Test),
                  BuildKeys.Build.Init(TaskUtils.NoOpTask),
                  BuildKeys.Build.DependsOn(BuildScope / BuildKeys.Build),
                  (BuildScope / BuildKeys.Build).Init(TaskUtils.NoOpTask),
                  (BuildScope / BuildKeys.Build).DependsOn(buildTargetKey / BuildKeys.Build));
    }

    protected abstract Settings Setup(Settings projectSettings, Key project);

    private Task BuildTaskImpl(IContext context, Key buildTarget) {
      context.Logger.Info("building...");
      return TaskUtils.NullAsyncResult;
    }
  }
}