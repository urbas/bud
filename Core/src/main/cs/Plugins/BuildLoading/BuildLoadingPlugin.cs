using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Commander;
using Bud.Plugins.Build;
using Bud.Plugins.CSharp;

namespace Bud.Plugins.BuildLoading {
  public class BuildLoadingPlugin {
    public static Setup Init(string dirOfProjectToBeBuilt) {
      return settings => settings
        .Do(
          Cs.Dll(),
          BuildLoadingKeys.CreateBuildCommander.Init(context => CreateBuildCommander(context, BuildUtils.BuildTargetKey(settings.Scope, BuildKeys.Main, CSharpKeys.CSharp)))
        )
        .In(BuildUtils.BuildTargetKey(settings.Scope, BuildKeys.Main, CSharpKeys.CSharp),
            BuildLoadingKeys.BuildConfigSourceFile.Init(context => Path.Combine(context.GetBaseDir(), "Build.cs")),
            BuildLoadingKeys.DirOfProjectToBeBuilt.Init(dirOfProjectToBeBuilt),
            CSharpKeys.SourceFiles.Modify(AddBuildDefinitionSourceFile),
            CSharpKeys.OutputAssemblyDir.Modify(BuildDirs.GetBaseDir),
            CSharpKeys.OutputAssemblyName.Modify("Build"),
            CSharpKeys.CollectReferencedAssemblies.Modify(AddBudAssemblies)
        );
    }

    public static async Task<IBuildCommander> CreateBuildCommander(IContext context, Key key) {
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

    public static async Task<IEnumerable<string>> AddBuildDefinitionSourceFile(IContext context, Func<Task<IEnumerable<string>>> previousSourcesTask, Key key) {
      var previousSources = await previousSourcesTask();
      return previousSources.Concat(new[] {context.GetBuildConfigSourceFile(key)});
    }

    private static async Task<ImmutableList<string>> AddBudAssemblies(IContext context, Func<Task<ImmutableList<string>>> existingAssemblies) {
      return (await existingAssemblies()).AddRange(BudAssemblies.GetBudAssembliesLocations());
    }
  }
}