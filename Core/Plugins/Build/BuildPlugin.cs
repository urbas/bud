using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Bud.Util;

namespace Bud.Plugins.Build {
  public abstract class BuildPlugin : IPlugin {
    protected readonly Key Scope;
    protected readonly Key Language;
    private readonly IEnumerable<IPlugin> plugins;

    protected BuildPlugin(Key scope, Key language, IEnumerable<IPlugin> plugins) {
      Scope = scope;
      Language = language;
      this.plugins = plugins ?? ImmutableList<IPlugin>.Empty;
    }

    public Settings ApplyTo(Settings settings, Key project) {
      var buildTargetKey = BuildTargetKey(project);
      return settings
        .Init(BuildDirsKeys.BaseDir.In(buildTargetKey), context => Path.Combine(context.GetBaseDir(project), "src", Scope.Id, Language.Id))
        .Init(BuildDirsKeys.OutputDir.In(buildTargetKey), context => Path.Combine(context.GetOutputDir(project), Scope.Id, Language.Id))
        .Init(BuildKeys.Build, TaskUtils.NoOpTask)
        .Init(BuildKeys.Build.In(Scope), TaskUtils.NoOpTask)
        .Init(BuildKeys.Build.In(buildTargetKey), context => InvokeCompilerTaskImpl(context, buildTargetKey))
        .Init(BuildKeys.Test, TaskUtils.NoOpTask)
        .Init(BuildKeys.Test.In(project), TaskUtils.NoOpTask)
        .AddDependencies(BuildKeys.Build, BuildKeys.Build.In(Scope))
        .AddDependencies(BuildKeys.Build.In(Scope), BuildKeys.Build.In(buildTargetKey))
        .AddDependencies(BuildKeys.Test, BuildKeys.Test.In(project))
        .Apply(buildTargetKey, PluginUtils.Create((existingsettings, buildTarget) => BuildTargetSettings(existingsettings, buildTarget, project)))
        .Apply(buildTargetKey, plugins);
    }

    protected abstract Settings BuildTargetSettings(Settings existingsettings, Key project, Key buildTarget);

    protected abstract Task<Unit> InvokeCompilerTaskImpl(IContext arg, Key buildKey);

    protected Key BuildTargetKey(Key project) {
      return BuildUtils.BuildTargetKey(project, Scope, Language);
    }
  }
}