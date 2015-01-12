using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Plugins.Build;
using Bud.Plugins.CSharp.Compiler;
using Bud.Plugins.Deps;

namespace Bud.Plugins.CSharp {
  public class CSharpBuildPlugin : BuildPlugin {
    public static readonly IPlugin ConvertBuildTargetToDll = PluginUtils.Create((existingSettings, buildTarget) => existingSettings.Modify(CSharpKeys.AssemblyType.In(buildTarget), assemblyType => AssemblyType.Library));

    public CSharpBuildPlugin(Key scope, params IPlugin[] plugins) : base(scope, CSharpKeys.CSharp, plugins) {}

    protected override Settings ApplyTo(Settings existingsettings, Key buildTarget, Key project) {
      return existingsettings
        .Init(CSharpKeys.SourceFiles.In(buildTarget), context => FindSources(context, buildTarget))
        .Init(CSharpKeys.AssemblyType.In(buildTarget), AssemblyType.Exe)
        .Init(CSharpKeys.CollectReferencedAssemblies.In(buildTarget), context => CollectAssembliesFromDependencies(context, buildTarget))
        .Init(CSharpKeys.OutputAssemblyDir.In(buildTarget), context => Path.Combine(context.GetOutputDir(buildTarget), "debug", "bin"))
        .Init(CSharpKeys.OutputAssemblyName.In(buildTarget), context => OutputAssemblyName(project, Scope))
        .Init(CSharpKeys.OutputAssemblyFile.In(buildTarget), context => Path.Combine(context.GetCSharpOutputAssemblyDir(buildTarget), string.Format("{0}.{1}", context.GetCSharpOutputAssemblyName(buildTarget), GetAssemblyFileExtension(context, buildTarget))));
    }

    private static string OutputAssemblyName(Key project, Key scope) {
      if (BuildKeys.Main.Equals(scope)) {
        return project.Id;
      }
      return project.Id + "." + scope.Id;
    }

    protected override Task<Unit> InvokeCompilerTaskImpl(IContext context, Key buildKey) {
      return CSharpCompiler.CompileProject(context, buildKey);
    }

    private IEnumerable<string> FindSources(IContext context, Key buildTarget) {
      var sourceDirectory = Path.Combine(context.GetBaseDir(buildTarget));
      if (Directory.Exists(sourceDirectory)) {
        return Directory.EnumerateFiles(sourceDirectory);
      }
      return ImmutableList<string>.Empty;
    }

    private async Task<ImmutableList<string>> CollectAssembliesFromDependencies(IContext context, Key buildTarget) {
      var internalDependencies = await context.ResolveInternalDependencies(buildTarget);
      var internalDependencyAssemblyPaths = CollectInternalDependencies(context, internalDependencies);
      var directNuGetDependencies = CollectExternalDependencies(context, buildTarget);
      var transitiveNuGetDependencies = internalDependencies.SelectMany(dependency => CollectExternalDependencies(context, dependency));
      return internalDependencyAssemblyPaths.AddRange(directNuGetDependencies).AddRange(transitiveNuGetDependencies);
    }

    private ImmutableList<string> CollectInternalDependencies(IContext context, IEnumerable<Key> dependencyBuildTargets) {
      var collectedAssemblies = ImmutableList.CreateBuilder<string>();
      foreach (var buildTarget in dependencyBuildTargets) {
        var cSharpOutputAssemblyFile = context.GetCSharpOutputAssemblyFile(buildTarget);
        collectedAssemblies.Add(cSharpOutputAssemblyFile);
      }
      return collectedAssemblies.ToImmutable();
    }

    private ImmutableList<string> CollectExternalDependencies(IConfig context, Key buildTarget) {
      var allNuGetDependencies = context.GetNuGetResolvedPackages();
      var nuGetDependencies = context.GetExternalDependencies(buildTarget);
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