using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Plugins.Build;
using Bud.Plugins.CSharp.Compiler;
using Bud.Plugins.Dependencies;

namespace Bud.Plugins.CSharp {
  public class CSharpBuildPlugin : BuildPlugin {
    public CSharpBuildPlugin(Key scope) : base(scope, CSharpKeys.CSharp) {}

    public override Settings ApplyTo(Settings settings, Key project) {
      var buildTarget = BuildTargetKey(project);
      return base.ApplyTo(settings, project)
                 .Init(CSharpKeys.SourceFiles.In(buildTarget), context => FindSources(context, project, buildTarget))
                 .Init(CSharpKeys.AssemblyType.In(buildTarget), AssemblyType.Exe)
                 .Init(CSharpKeys.CollectReferencedAssemblies.In(buildTarget), context => CollectAssembliesFromDependencies(context, buildTarget))
                 .Init(CSharpKeys.OutputAssemblyDir.In(buildTarget), context => Path.Combine(context.GetOutputDir(project), ".net-4.5", Scope.Id, "debug", "bin"))
                 .Init(CSharpKeys.OutputAssemblyName.In(buildTarget), context => project.Id)
                 .Init(CSharpKeys.OutputAssemblyFile.In(buildTarget), context => Path.Combine(context.GetCSharpOutputAssemblyDir(buildTarget), string.Format("{0}.{1}", context.GetCSharpOutputAssemblyName(buildTarget), context.GetAssemblyFileExtension(buildTarget))));
    }

    protected override Task<Unit> Build(IContext context, Key buildKey) {
      return CSharpCompiler.CompileProject(context, buildKey);
    }

    private static IEnumerable<string> FindSources(IContext context, Key project, Key buildTarget) {
      var sourceDirectory = Path.Combine(context.GetBaseDir(project), "src", "main", "cs");
      if (Directory.Exists(sourceDirectory)) {
        return Directory.EnumerateFiles(sourceDirectory);
      }
      return ImmutableList<string>.Empty;
    }

    private async Task<ImmutableList<string>> CollectAssembliesFromDependencies(IContext context, Key buildKey) {
      var internalDependencies = await context.ResolveInternalDependencies(buildKey, CSharpKeys.CSharp);
      var internalDependencyAssemblyPaths = CollectInternalDependencies(context, internalDependencies);
      var directNuGetDependencies = CollectExternalDependencies(context, buildKey);
      var transitiveNuGetDependencies = internalDependencies.SelectMany(dependency => CollectExternalDependencies(context, dependency));
      return internalDependencyAssemblyPaths.AddRange(directNuGetDependencies).AddRange(transitiveNuGetDependencies);
    }

    private ImmutableList<string> CollectInternalDependencies(IContext context, IEnumerable<Key> dependencyProjects) {
      var collectedAssemblies = ImmutableList.CreateBuilder<string>();
      foreach (var dependency in dependencyProjects) {
        collectedAssemblies.Add(context.GetCSharpOutputAssemblyFile(dependency));
      }
      return collectedAssemblies.ToImmutable();
    }

    private static ImmutableList<string> CollectExternalDependencies(IConfig context, Key buildKey) {
      var allNuGetDependencies = context.GetNuGetResolvedPackages();
      var nuGetDependencies = context.GetExternalDependencies(buildKey, CSharpKeys.CSharp);
      var nuGetRepositoryPath = context.GetNuGetRepositoryDir();
      return nuGetDependencies
        .Select(dependency => allNuGetDependencies.GetResolvedNuGetDependency(dependency))
        .SelectMany(dependency => dependency.AssemblyPaths.Select(assemblyPath => Path.Combine(nuGetRepositoryPath, dependency.RequestedDependency.Id + "." + dependency.ResolvedVersion, assemblyPath))).ToImmutableList();
    }
  }
}