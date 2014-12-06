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

namespace Bud.Plugins.BuildLoading
{
  public class BuildLoadingPlugin : IPlugin	{
    private readonly string dirOfProjectToBeBuilt;
    private readonly string budAssembliesDir;

    public BuildLoadingPlugin(string dirOfProjectToBeBuilt, string budAssembliesDir) {
      this.budAssembliesDir = budAssembliesDir;
      this.dirOfProjectToBeBuilt = dirOfProjectToBeBuilt;
    }

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings
        .Add(CSharpPlugin.Instance)
        .Init(BuildLoadingKeys.BuildConfigSourceFile.In(scope), context => Path.Combine(context.GetBaseDir(scope), "Build.cs"))
        .Init(BuildLoadingKeys.DirOfProjectToBeBuilt.In(scope), dirOfProjectToBeBuilt)
        .Modify(CSharpKeys.SourceFiles.In(scope), (context, previousTask) => AddBuildDefinitionSourceFile(context, previousTask, scope))
        .Modify(CSharpKeys.OutputAssemblyDir.In(scope), (context, previousValue) => context.GetBaseDir(scope))
        .Modify(CSharpKeys.OutputAssemblyName.In(scope), (context, previousValue) => "Build")
        .InitOrKeep(BuildLoadingKeys.LoadBuildSettings.In(scope), context => LoadOrBuildSettings(context, scope))
        .Modify(CSharpKeys.AssemblyType.In(scope), prevValue => AssemblyType.Library)
        .Modify(CSharpKeys.ReferencedAssemblies.In(scope), assemblies => assemblies.AddRange(GetBudAssemblies(budAssembliesDir)));
    }

    public async Task<Settings> LoadOrBuildSettings(EvaluationContext context, Scope scope) {
      var buildConfigSourceFile = context.GetBuildConfigSourceFile(scope);
      var dirOfProjectToBeBuilt = context.GetDirOfProjectToBeBuilt(scope);
      // TODO: Check if the BakedBuild.dll file exists. If it does, just load it.
      if (File.Exists(buildConfigSourceFile)) {
        await context.BuildAll();
        var objectHandle = Activator.CreateInstanceFrom(context.GetCSharpOutputAssemblyFile(scope), "Build");
        var buildDefinition = (IBuild)objectHandle.Unwrap();
        return buildDefinition.GetSettings(dirOfProjectToBeBuilt);
      } else {
        return CSharp.CSharp.Project(Path.GetFileName(dirOfProjectToBeBuilt), dirOfProjectToBeBuilt);
      }
    }

    public async Task<IEnumerable<string>> AddBuildDefinitionSourceFile(EvaluationContext context, Func<Task<IEnumerable<string>>> previousSourcesTask, Scope scope) {
      var previousSources = await previousSourcesTask();
      return previousSources.Concat(new []{ context.GetBuildConfigSourceFile(scope) });
    }

    public static IEnumerable<string> GetBudAssemblies(string assembliesDir) {
      return new [] {
        "Bud.Graph.dll",
        "Bud.Core.dll"
      }.Select(assemblyName => Path.Combine(assembliesDir, assemblyName));
    }
  }

}

