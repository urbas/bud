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
                 .Init(CSharpKeys.SourceFiles.In(buildTarget), context => FindSources(context, project))
                 .Init(CSharpKeys.AssemblyType.In(buildTarget), AssemblyType.Exe)
                 .Init(CSharpKeys.CollectReferencedAssemblies.In(buildTarget), context => CollectAssembliesFromDependencies(context, project))
                 .Init(CSharpKeys.OutputAssemblyDir.In(buildTarget), context => Path.Combine(context.GetOutputDir(project), ".net-4.5", Scope.Id, "debug", "bin"))
                 .Init(CSharpKeys.OutputAssemblyName.In(buildTarget), context => project.Id)
                 .Init(CSharpKeys.OutputAssemblyFile.In(buildTarget), context => Path.Combine(context.GetCSharpOutputAssemblyDir(buildTarget), string.Format("{0}.{1}", context.GetCSharpOutputAssemblyName(buildTarget), GetAssemblyFileExtension(context, buildTarget))));
    }

    protected override Task<Unit> Build(IContext context, Key buildKey) {
      return CSharpCompiler.CompileProject(context, buildKey);
    }

    private IEnumerable<string> FindSources(IContext context, Key project) {
      var sourceDirectory = Path.Combine(context.GetBaseDir(project), "src", Scope.Id, CSharpKeys.CSharp.Id);
      if (Directory.Exists(sourceDirectory)) {
        return Directory.EnumerateFiles(sourceDirectory);
      }
      return ImmutableList<string>.Empty;
    }

    private async Task<ImmutableList<string>> CollectAssembliesFromDependencies(IContext context, Key thisProject) {
      var internalDependencies = await context.ResolveInternalDependencies(thisProject, CSharp.MainCSharpDependencyType);
      var internalDependencyAssemblyPaths = CollectInternalDependencies(context, internalDependencies);
      var directNuGetDependencies = CollectExternalDependencies(context, thisProject);
      var transitiveNuGetDependencies = internalDependencies.SelectMany(dependency => CollectExternalDependencies(context, dependency));
      return internalDependencyAssemblyPaths.AddRange(directNuGetDependencies).AddRange(transitiveNuGetDependencies);
    }

    private ImmutableList<string> CollectInternalDependencies(IContext context, IEnumerable<Key> dependencyProjects) {
      var collectedAssemblies = ImmutableList.CreateBuilder<string>();
      foreach (var project in dependencyProjects) {
        collectedAssemblies.Add(context.GetCSharpOutputAssemblyFile(CSharp.MainBuildTargetKey(project)));
      }
      return collectedAssemblies.ToImmutable();
    }

    private static ImmutableList<string> CollectExternalDependencies(IConfig context, Key thisProject) {
      var allNuGetDependencies = context.GetNuGetResolvedPackages();
      var nuGetDependencies = context.GetExternalDependencies(thisProject, CSharp.MainCSharpDependencyType);
      var nuGetRepositoryPath = context.GetNuGetRepositoryDir();
      return nuGetDependencies
        .Select(dependency => allNuGetDependencies.GetResolvedNuGetDependency(dependency))
        .SelectMany(dependency => dependency.AssemblyPaths.Select(assemblyPath => Path.Combine(nuGetRepositoryPath, dependency.RequestedDependency.Id + "." + dependency.ResolvedVersion, assemblyPath))).ToImmutableList();
    }

    private static string GetAssemblyFileExtension(IConfig context, Key project) {
      switch (context.GetCSharpAssemblyType(project)) {
        case AssemblyType.Exe:
          return "exe";
        case AssemblyType.Library:
          return "dll";
        case AssemblyType.WinExe:
          return "exe";
        case AssemblyType.Module:
          return "module";
        default:
          throw new ArgumentException("Unsupported assembly type.");
      }
    }
  }
}