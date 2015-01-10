using System;
using System.Threading.Tasks;
using Bud.Util;

namespace Bud.Plugins.Build {
  public abstract class BuildPlugin : IPlugin {
    protected readonly Key Scope;
    protected readonly Key Language;

    protected BuildPlugin(Key scope, Key language) {
      Scope = scope;
      Language = language;
    }

    public virtual Settings ApplyTo(Settings settings, Key project) {
      var buildKey = BuildTargetKey(project);
      return settings
        .Init(BuildKeys.Build, TaskUtils.NoOpTask)
        .Init(BuildKeys.Build.In(Scope), TaskUtils.NoOpTask)
        .Init(BuildKeys.Build.In(buildKey), context => Build(context, buildKey))
        .Init(BuildKeys.Test, TaskUtils.NoOpTask)
        .Init(BuildKeys.Test.In(project), TaskUtils.NoOpTask)
        .AddDependencies(BuildKeys.Build, BuildKeys.Build.In(Scope))
        .AddDependencies(BuildKeys.Build.In(Scope), BuildKeys.Build.In(buildKey))
        .AddDependencies(BuildKeys.Test, BuildKeys.Test.In(project));
    }

    protected abstract Task<Unit> Build(IContext arg, Key buildKey);

    protected Key BuildTargetKey(Key project) {
      return BuildUtils.BuildTargetKey(project, Scope, Language);
    }
  }
}