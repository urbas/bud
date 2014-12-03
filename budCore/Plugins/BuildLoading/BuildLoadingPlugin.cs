using System;
using System.Linq;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using Bud.Plugins.CSharp;
using Bud.Plugins.Build;
using System.IO;
using System.Collections.Generic;
using Bud;
using System.Threading.Tasks;

namespace Bud.Plugins.BuildLoading
{
  public class BuildLoadingPlugin : BudPlugin	{
    private readonly string dirOfProjectToBeBuilt;

    public BuildLoadingPlugin(string dirOfProjectToBeBuilt) {
      this.dirOfProjectToBeBuilt = dirOfProjectToBeBuilt;
    }

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings
        .Add(CSharpPlugin.Instance)
        .InitOrKeep(BuildLoadingKeys.DirOfProjectToBeBuilt.In(scope), dirOfProjectToBeBuilt)
        .Modify(CSharpKeys.SourceFiles.In(scope), (context, previousTask) => AddBuildDefinitionSourceFile(context, previousTask, scope))
        .InitOrKeep(BuildLoadingKeys.LoadBuildSettings.In(scope), context => LoadOrBuildSettings(context, scope));
    }

    private async Task<Settings> LoadOrBuildSettings(EvaluationContext context, Scope scope) {
      await context.BuildAll();
      return CSharp.CSharp.Project("root", context.GetDirOfProjectToBeBuilt(scope));
    }

    private async Task<IEnumerable<string>> AddBuildDefinitionSourceFile(EvaluationContext context, Func<Task<IEnumerable<string>>> previousSourcesTask, Scope scope) {
      var previousSources = await previousSourcesTask();
      return previousSources.Concat(new []{ Path.Combine(context.GetBaseDir(scope), "Build.cs") });
    }
  }

}

