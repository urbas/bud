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

    public Settings ApplyTo(Settings settings, Key key) {
      return settings
        .Apply(key, CSharpPlugin.Instance)
        .Init(BuildLoadingKeys.BuildConfigSourceFile.In(key), context => Path.Combine(context.GetBaseDir(key), "Build.cs"))
        .Init(BuildLoadingKeys.DirOfProjectToBeBuilt.In(key), dirOfProjectToBeBuilt)
        .Modify(CSharpKeys.SourceFiles.In(key), (context, previousTask) => AddBuildDefinitionSourceFile(context, previousTask, key))
        .Modify(CSharpKeys.OutputAssemblyDir.In(key), (context, previousValue) => context.GetBaseDir(key))
        .Modify(CSharpKeys.OutputAssemblyName.In(key), (context, previousValue) => "Build")
        .Init(BuildLoadingKeys.CreateBuildCommander.In(key), context => CreateBuildCommander(context, key))
        .Modify(CSharpKeys.AssemblyType.In(key), prevValue => AssemblyType.Library)
        .Modify(CSharpKeys.CollectReferencedAssemblies.In(key), async (context, assemblies) => (await assemblies()).AddRange(BudAssemblies.GetBudAssembliesLocations()));
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

