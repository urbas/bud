using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Build;
using Bud.CSharp;
using Bud.Logging;
using Bud.Projects;
using NuGet;

namespace Bud.Commander {
  public static class BuildCommander {
    private const string BuildDefinitionProjectId = "BuildDefinition";

    public static IBuildCommander LoadProjectLevelCommander(string path) {
      var buildSettings = LoadBuildLevelSettings(path);
      var evaluationContext = Context.FromSettings(buildSettings, Logger.CreateFromStandardOutputs());
      var buildCommanderTask = CreateBuildCommander(evaluationContext, ProjectPlugin.ProjectKey(BuildDefinitionProjectId));
      buildCommanderTask.Wait();
      return buildCommanderTask.Result;
    }

    private static Settings LoadBuildLevelSettings(string path) {
      var buildProjectDir = Path.Combine(path, BuildDirs.BudDirName);
      return GlobalBuild.New(buildProjectDir).Project(BuildDefinitionProjectId, buildProjectDir, Init(path));
    }

    private static Setup Init(string dirOfProjectToBeBuilt) {
      return Settings.Modify(
        BuildCommanderKeys.CreateBuildCommander.Init(CreateBuildCommanderImpl),
        Cs.Dll(BuildCommanderKeys.BuildConfigSourceFile.Init(GetDefaultBuildSourceFile),
               BuildCommanderKeys.DirOfProjectToBeBuilt.Init(dirOfProjectToBeBuilt),
               BuildTargetKeys.SourceFiles.Modify(AddBuildDefinitionSourceFile),
               CSharpKeys.OutputAssemblyDir.Modify(BuildDirs.GetBaseDir),
               CSharpKeys.OutputAssemblyName.Modify("Build"),
               BuildCommanderPlugin.BudAssemblyReferences));
    }

    private static async Task<IBuildCommander> CreateBuildCommander(IContext context, Key buildLoadingProject) {
      return await context.Evaluate(buildLoadingProject / BuildCommanderKeys.CreateBuildCommander);
    }

    private static string GetBuildConfigSourceFile(this IContext context, Key buildLoadingProject) {
      return context.Evaluate(buildLoadingProject / BuildCommanderKeys.BuildConfigSourceFile);
    }

    public static string GetDirOfProjectToBeBuilt(this IContext context, Key buildLoadingProject) {
      return context.Evaluate(buildLoadingProject / BuildCommanderKeys.DirOfProjectToBeBuilt);
    }

    private static string GetDefaultBuildSourceFile(IConfig context) {
      return Path.Combine(context.GetBaseDir(), "Build.cs");
    }

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

    public static IBuildCommander LoadBuildLevelCommander(string projectLevelDir) {
      return new DefaultBuildCommander(LoadBuildLevelSettings(projectLevelDir));
    }
  }
}