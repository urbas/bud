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
      var buildTargetKey = BuildTargetKey(project);
      return projectSettings
        .Do(BuildKeys.Test.Init(TaskUtils.NoOpTask))
        .In(buildTargetKey,
            BuildDirsKeys.BaseDir.Init(context => Path.Combine(context.GetBaseDir(project), "src", BuildScope.Id, Language.Id)),
            BuildDirsKeys.OutputDir.Init(context => Path.Combine(context.GetOutputDir(project), BuildScope.Id, Language.Id)),
            BuildKeys.Build.Init(BuildTaskImpl),
            buildTargetSettings => Setup(buildTargetSettings, project),
            setups.ToPlugin())
        .Globally(BuildKeys.Test.Init(TaskUtils.NoOpTask),
                  BuildKeys.Test.DependsOn(BuildKeys.Test.In(project)),
                  BuildKeys.Build.Init(TaskUtils.NoOpTask),
                  BuildKeys.Build.DependsOn(BuildKeys.Build.In(BuildScope)),
                  BuildKeys.Build.In(BuildScope).Init(TaskUtils.NoOpTask),
                  BuildKeys.Build.In(BuildScope).DependsOn(BuildKeys.Build.In(buildTargetKey)));
    }

    protected abstract Settings Setup(Settings projectSettings, Key project);

    private Task BuildTaskImpl(IContext context, Key buildTarget) {
      context.Logger.Info(string.Format("{0}: building scope '{1}' language '{2}'...", BuildUtils.ProjectOf(buildTarget).Id, BuildUtils.ScopeOf(buildTarget).Id, BuildUtils.LanguageOf(buildTarget).Id));
      return TaskUtils.NullAsyncResult;
    }

    protected Key BuildTargetKey(Key project) {
      return project / BuildScope / Language;
    }
  }
}