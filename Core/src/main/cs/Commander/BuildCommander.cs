using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Plugins.Build;
using Bud.Plugins.CSharp;
using Bud.Plugins.Projects;

namespace Bud.Commander {
  public static class BuildCommander {
    public static IBuildCommander Load(string path) {
      var buildProjectDir = Path.Combine(path, BuildDirs.BudDirName);
      const string buildProjectId = "BuildDefinition";
      var buildProject = BuildProject(GlobalBuild.New(buildProjectDir), buildProjectId, buildProjectDir, path);
      var evaluationContext = Context.FromSettings(buildProject);
      var buildCommanderTask = CreateBuildCommander(evaluationContext, ProjectsSettings.ProjectKey(buildProjectId));
      buildCommanderTask.Wait();
      return buildCommanderTask.Result;
    }

    private static Settings BuildProject(Settings build, string projectId, string budDir, string dirOfProjectToBeBuilt) {
      return build.Project(projectId, budDir, Init(dirOfProjectToBeBuilt));
    }

    private static async Task<IBuildCommander> CreateBuildCommander(IContext context, Key buildLoadingProject) {
      return await context.Evaluate(BuildCommanderKeys.CreateBuildCommander.In(buildLoadingProject));
    }

    private static Setup Init(string dirOfProjectToBeBuilt) {
      return Settings.Modify(
        BuildCommanderKeys.CreateBuildCommander.Init(CreateBuildCommanderImpl),
        Cs.Dll(BuildCommanderKeys.BuildConfigSourceFile.Init(GetDefaultBuildSourceFile),
               BuildCommanderKeys.DirOfProjectToBeBuilt.Init(dirOfProjectToBeBuilt),
               CSharpKeys.SourceFiles.Modify(AddBuildDefinitionSourceFile),
               CSharpKeys.OutputAssemblyDir.Modify(BuildDirs.GetBaseDir),
               CSharpKeys.OutputAssemblyName.Modify("Build"),
               CSharpKeys.CollectReferencedAssemblies.Modify(AddBudAssemblies)));
    }

    private static string GetBuildConfigSourceFile(this IContext context, Key buildLoadingProject) {
      return context.Evaluate(BuildCommanderKeys.BuildConfigSourceFile.In(buildLoadingProject));
    }

    public static string GetDirOfProjectToBeBuilt(this IContext context, Key buildLoadingProject) {
      return context.Evaluate(BuildCommanderKeys.DirOfProjectToBeBuilt.In(buildLoadingProject));
    }

    private static string GetDefaultBuildSourceFile(IConfig context) {
      return Path.Combine(context.GetBaseDir(), "Build.cs");
    }

    private static async Task<IBuildCommander> CreateBuildCommanderImpl(IContext context, Key project) {
      var buildTarget = BuildUtils.BuildTarget(project, BuildKeys.Main, CSharpKeys.CSharp);
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

    private static async Task<ImmutableList<string>> AddBudAssemblies(IContext context, Func<Task<ImmutableList<string>>> existingAssemblies) {
      return (await existingAssemblies()).AddRange(BudAssemblies.GetBudAssembliesLocations());
    }
  }
}