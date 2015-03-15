using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Path;
using System.Linq;
using System.Threading.Tasks;
using Bud.Build;
using Bud.Commander;
using Bud.CSharp;
using Bud.Projects;
using NuGet;

namespace Bud.BuildDefinition {
  public static class BuildDefinitionPlugin {
    public const string BuildDefinitionProjectId = "BuildDefinition";
    public const string BuildDefinitionAssemblyName = "Build";
    public const string BuildDefinitionMainSourceFile = "Build.cs";
    public static readonly Key BuildDefinitionProjectKey = ProjectPlugin.ProjectKey(BuildDefinitionPlugin.BuildDefinitionProjectId);

    public static Setup AddToProject(string dirOfProjectToBeBuilt) {
      return Settings.Modify(BuildDefinitionKeys.CreateBuildCommander.Init(CreateBuildCommanderImpl),
                             Cs.Dll(BuildDefinitionKeys.BuildConfigSourceFile.Init(GetDefaultBuildSourceFile),
                                    BuildDefinitionKeys.DirOfProjectToBeBuilt.Init(dirOfProjectToBeBuilt),
                                    BuildTargetKeys.SourceFiles.Modify(AddBuildDefinitionSourceFile),
                                    CSharpKeys.OutputAssemblyDir.Modify(BuildDirs.GetBaseDir),
                                    CSharpKeys.OutputAssemblyName.Modify(BuildDefinitionAssemblyName),
                                    BudAssemblyReferences));
    }

    public static Setup BudAssemblyReferences => CSharpKeys.AssemblyReferences.Modify(AddBudAssembliesImpl);

    private static string GetDefaultBuildSourceFile(IConfig context) => Combine(context.GetBaseDir(), BuildDefinitionMainSourceFile);

    private static async Task<IBuildCommander> CreateBuildCommanderImpl(IContext context, Key project) {
      var buildTarget = project / BuildKeys.Main / CSharpKeys.CSharp;
      var buildConfigSourceFile = context.GetBuildConfigSourceFile(buildTarget);
      var dirOfProjectToBeBuilt = context.GetDirOfProjectToBeBuilt(buildTarget);
      // TODO: Check if the BakedBuild.dll file exists. If it does, just load it.
      if (File.Exists(buildConfigSourceFile)) {
        await context.Evaluate(BuildKeys.Build);
        return new AppDomainBuildCommander(context.GetCSharpOutputAssemblyFile(buildTarget), dirOfProjectToBeBuilt);
      }
      return new DefaultBuildCommander(dirOfProjectToBeBuilt);
    }

    private static async Task<IEnumerable<string>> AddBuildDefinitionSourceFile(IContext context, Func<Task<IEnumerable<string>>> previousSourcesTask, Key key) {
      var previousSources = await previousSourcesTask();
      return previousSources.Concat(new[] {context.GetBuildConfigSourceFile(key)});
    }

    private static IEnumerable<IPackageAssemblyReference> AddBudAssembliesImpl(IConfig config, IEnumerable<IPackageAssemblyReference> existingAssemblies) {
      return existingAssemblies.Concat(BudAssemblies.AssemblyReferences);
    }
  }
}