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

    public Settings Setup(Settings settings) {
      var project = settings.Scope;
      var buildTargetKey = BuildTargetKey(project);
      return settings
        .Do(BuildKeys.Test.Init(TaskUtils.NoOpTask))
        .In(buildTargetKey,
            BuildDirsKeys.BaseDir.Init(context => Path.Combine(context.GetBaseDir(project), "src", BuildScope.Id, Language.Id)),
            BuildDirsKeys.OutputDir.Init(context => Path.Combine(context.GetOutputDir(project), BuildScope.Id, Language.Id)),
            BuildKeys.Build.Init(BuildTaskImpl),
            existingsettings => Setup(existingsettings, project),
            setups.ToPlugin())
        .Globally(BuildKeys.Test.Init(TaskUtils.NoOpTask),
                  BuildKeys.Test.DependsOn(BuildKeys.Test.In(project)),
                  BuildKeys.Build.Init(TaskUtils.NoOpTask),
                  BuildKeys.Build.DependsOn(BuildKeys.Build.In(BuildScope)),
                  BuildKeys.Build.In(BuildScope).Init(TaskUtils.NoOpTask),
                  BuildKeys.Build.In(BuildScope).DependsOn(BuildKeys.Build.In(buildTargetKey)));
    }

    protected abstract Settings Setup(Settings existingsettings, Key project);

    protected abstract Task BuildTaskImpl(IContext arg, Key buildTarget);

    protected Key BuildTargetKey(Key project) {
      return BuildUtils.BuildTarget(project, BuildScope, Language);
    }
  }
}