using System;
using System.Linq;
using Bud.Plugins;
using System.Collections.Immutable;
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

    public Settings ApplyTo(Settings settings) {
      var project = settings.Scope;
      var buildTarget = BuildUtils.BuildTargetKey(project, BuildKeys.Main, CSharpKeys.CSharp);
      return settings
        .Apply(project, Cs.Dll())
        .Do(
          BuildLoadingKeys.CreateBuildCommander.Init(context => CreateBuildCommander(context, buildTarget))
        )
        .In(buildTarget,
            BuildLoadingKeys.BuildConfigSourceFile.Init(context => Path.Combine(context.GetBaseDir(), "Build.cs")),
            BuildLoadingKeys.DirOfProjectToBeBuilt.Init(dirOfProjectToBeBuilt),
            CSharpKeys.SourceFiles.Modify(AddBuildDefinitionSourceFile),
            CSharpKeys.OutputAssemblyDir.Modify(BuildDirs.GetBaseDir),
            CSharpKeys.OutputAssemblyName.Modify("Build"),
            CSharpKeys.CollectReferencedAssemblies.Modify(AddBudAssemblies)
        );
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

    private static async Task<ImmutableList<string>> AddBudAssemblies(IContext context, Func<Task<ImmutableList<string>>> existingAssemblies) {
      return (await existingAssemblies()).AddRange(BudAssemblies.GetBudAssembliesLocations());
    }
  }

}

