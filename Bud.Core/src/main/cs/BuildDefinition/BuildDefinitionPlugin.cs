using System;
using System.Collections.Generic;
using System.IO.Path;
using System.Linq;
using System.Threading.Tasks;
using Bud.Build;
using Bud.CSharp;
using Bud.Projects;
using NuGet;

namespace Bud.BuildDefinition {
  public class BuildDefinitionPlugin : Plugin {
    public const string BuildDefinitionProjectId = "BuildDefinition";
    public const string BuildDefinitionAssemblyName = "Build";
    public const string BuildDefinitionSourceFileName = "Build.cs";
    public const string BuildDefinitionAssemblyFileName = "Build.dll";
    public static readonly Key BuildDefinitionProjectKey = Project.ProjectKey(BuildDefinitionProjectId);

    public override Settings Setup(Settings settings) {
      return settings.Add(Cs.Dll(BuildDefinitionKeys.BuildConfigSourceFile.Init(GetDefaultBuildSourceFile),
                                 BuildTargetKeys.SourceFiles.Modify(AddBuildDefinitionSourceFile),
                                 Cs.OutputAssemblyDir.Modify(BuildDirs.GetBaseDir),
                                 Cs.OutputAssemblyName.Modify(BuildDefinitionAssemblyName),
                                 Cs.DistDir.Modify(BuildDirs.GetBaseDir),
                                 AddBudAssemblyReferences));
    }

    public static Setup AddBudAssemblyReferences => Cs.AssemblyReferences.Modify(AddBudAssembliesImpl);

    private static string GetDefaultBuildSourceFile(IConfig context) => Combine(context.GetBaseDir(), BuildDefinitionSourceFileName);

    private static async Task<IEnumerable<string>> AddBuildDefinitionSourceFile(IContext context, Func<Task<IEnumerable<string>>> previousSourcesTask, Key key) {
      var previousSources = await previousSourcesTask();
      return previousSources.Concat(new[] {context.GetBuildConfigSourceFile(key)});
    }

    private static IEnumerable<IPackageAssemblyReference> AddBudAssembliesImpl(IConfig config, IEnumerable<IPackageAssemblyReference> existingAssemblies) {
      return existingAssemblies.Concat(BudAssemblies.AssemblyReferences).Concat(BudAssemblies.CoreDependenciesReferences);
    }
  }
}