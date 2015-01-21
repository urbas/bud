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

    public Settings ApplyTo(Settings settings, Key project) {
      var buildTarget = BuildUtils.BuildTargetKey(project, BuildKeys.Main, CSharpKeys.CSharp);
      return settings
        .Apply(project, CSharpPlugin.Instance)
        .Init(BuildLoadingKeys.CreateBuildCommander.In(project), context => CreateBuildCommander(context, buildTarget))
        .Init(BuildLoadingKeys.BuildConfigSourceFile.In(buildTarget), context => Path.Combine(context.GetBaseDir(), "Build.cs"))
        .Init(BuildLoadingKeys.DirOfProjectToBeBuilt.In(buildTarget), dirOfProjectToBeBuilt)
        .Modify(CSharpKeys.SourceFiles.In(buildTarget), (context, previousTask) => AddBuildDefinitionSourceFile(context, previousTask, buildTarget))
        .Modify(CSharpKeys.OutputAssemblyDir.In(buildTarget), (context, previousValue) => context.GetBaseDir())
        .Modify(CSharpKeys.OutputAssemblyName.In(buildTarget), (context, previousValue) => "Build")
        .Apply(buildTarget, CSharpBuildTargetPlugin.ConvertBuildTargetToDll)
        .Modify(CSharpKeys.CollectReferencedAssemblies.In(buildTarget), async (context, assemblies) => (await assemblies()).AddRange(BudAssemblies.GetBudAssembliesLocations()));
    }

    public async Task<IBuildCommander> CreateBuildCommander(IContext context, Key key) {
      var buildConfigSourceFile = context.GetBuildConfigSourceFile(key);
      var dirOfProjectToBeBuilt = context.GetDirOfProjectToBeBuilt(key);
      // TODO: Check if the BakedBuild.dll file exists. If it does, just load it.
      if (File.Exists(buildConfigSourceFile)) {
        await context.Evaluate(BuildKeys.Build);
        return new AppDomainBuildCommander(context.GetCSharpOutputAssemblyFile(key), dirOfProjectToBeBuilt);
      } else {
        return new DefaultBuildCommander(dirOfProjectToBeBuilt);
      }
    }

    public async Task<IEnumerable<string>> AddBuildDefinitionSourceFile(IContext context, Func<Task<IEnumerable<string>>> previousSourcesTask, Key key) {
      var previousSources = await previousSourcesTask();
      return previousSources.Concat(new []{ context.GetBuildConfigSourceFile(key) });
    }
  }

}

