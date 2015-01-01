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
using System.Reflection;
using Bud.Commander;

namespace Bud.Plugins.BuildLoading {
  public class BuildLoadingPlugin : IPlugin {
    private readonly string dirOfProjectToBeBuilt;

    public BuildLoadingPlugin(string dirOfProjectToBeBuilt) {
      this.dirOfProjectToBeBuilt = dirOfProjectToBeBuilt;
    }

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings
        .Apply(scope, CSharpPlugin.Instance)
        .Init(BuildLoadingKeys.BuildConfigSourceFile.In(scope), context => Path.Combine(context.GetBaseDir(scope), "Build.cs"))
        .Init(BuildLoadingKeys.DirOfProjectToBeBuilt.In(scope), dirOfProjectToBeBuilt)
        .Modify(CSharpKeys.SourceFiles.In(scope), (context, previousTask) => AddBuildDefinitionSourceFile(context, previousTask, scope))
        .Modify(CSharpKeys.OutputAssemblyDir.In(scope), (context, previousValue) => context.GetBaseDir(scope))
        .Modify(CSharpKeys.OutputAssemblyName.In(scope), (context, previousValue) => "Build")
        .Init(BuildLoadingKeys.CreateBuildCommander.In(scope), context => CreateBuildCommander(context, scope))
        .Modify(CSharpKeys.AssemblyType.In(scope), prevValue => AssemblyType.Library)
        .Modify(CSharpKeys.CollectReferencedAssemblies.In(scope), async (context, assemblies) => (await assemblies()).AddRange(BudAssemblies.GetBudAssembliesLocations()));
    }

    public async Task<IBuildCommander> CreateBuildCommander(EvaluationContext context, Scope scope) {
      var buildConfigSourceFile = context.GetBuildConfigSourceFile(scope);
      var dirOfProjectToBeBuilt = context.GetDirOfProjectToBeBuilt(scope);
      // TODO: Check if the BakedBuild.dll file exists. If it does, just load it.
      if (File.Exists(buildConfigSourceFile)) {
        await context.BuildAll();
        return new AppDomainBuildCommander(context.GetCSharpOutputAssemblyFile(scope), dirOfProjectToBeBuilt);
      } else {
        return new DefaultBuildCommander(dirOfProjectToBeBuilt);
      }
    }

    public async Task<IEnumerable<string>> AddBuildDefinitionSourceFile(EvaluationContext context, Func<Task<IEnumerable<string>>> previousSourcesTask, Scope scope) {
      var previousSources = await previousSourcesTask();
      return previousSources.Concat(new []{ context.GetBuildConfigSourceFile(scope) });
    }
  }

}

