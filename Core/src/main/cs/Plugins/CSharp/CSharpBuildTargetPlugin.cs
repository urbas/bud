using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Plugins.Build;
using Bud.Plugins.CSharp.Compiler;
using Bud.Plugins.Deps;
using Bud.Util;
using NuGet;

namespace Bud.Plugins.CSharp {
  public class CSharpBuildTargetPlugin : BuildTargetPlugin {
    public static readonly IPlugin ConvertBuildTargetToDll = PluginUtils.Create((existingSettings, buildTarget) => existingSettings.Modify(CSharpKeys.AssemblyType.In(buildTarget), assemblyType => AssemblyType.Library));

    public CSharpBuildTargetPlugin(Key scope, params IPlugin[] plugins) : base(scope, CSharpKeys.CSharp, plugins) {}

    protected override Settings ApplyTo(Settings existingsettings, Key buildTarget, Key project) {
      return existingsettings
        .Apply(buildTarget, DependenciesPlugin.Instance)
        .Init(CSharpKeys.SourceFiles.In(buildTarget), context => FindSources(context, buildTarget))
        .Init(CSharpKeys.TargetFramework.In(buildTarget), Framework.Net45)
        .Init(CSharpKeys.AssemblyType.In(buildTarget), AssemblyType.Exe)
        .Init(CSharpKeys.CollectReferencedAssemblies.In(buildTarget), context => CollectAssembliesFromDependencies(context, buildTarget))
        .Init(CSharpKeys.OutputAssemblyDir.In(buildTarget), context => Path.Combine(context.GetOutputDir(buildTarget), "debug", "bin"))
        .Init(CSharpKeys.OutputAssemblyName.In(buildTarget), context => OutputAssemblyName(project, Scope))
        .Init(CSharpKeys.Dist.In(buildTarget), context => CreateDistributablePackage(context, buildTarget))
        .Init(CSharpKeys.OutputAssemblyFile.In(buildTarget), context => Path.Combine(context.GetCSharpOutputAssemblyDir(buildTarget), string.Format("{0}.{1}", context.GetCSharpOutputAssemblyName(buildTarget), GetAssemblyFileExtension(context, buildTarget))));
    }

    private static string OutputAssemblyName(Key project, Key scope) {
      if (BuildKeys.Main.Equals(scope)) {
        return project.Id;
      }
      return project.Id + "." + StringUtils.Capitalize(scope.Id);
    }

    protected override Task<Unit> InvokeCompilerTaskImpl(IContext context, Key buildKey) {
      return CSharpCompiler.CompileProject(context, buildKey);
    }

    private IEnumerable<string> FindSources(IContext context, Key buildTarget) {
      var sourceDirectory = Path.Combine(context.GetBaseDir(buildTarget));
      if (Directory.Exists(sourceDirectory)) {
        return Directory.EnumerateFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories);
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
        if (File.Exists(cSharpOutputAssemblyFile)) {
          collectedAssemblies.Add(cSharpOutputAssemblyFile);
        }
      }
      return collectedAssemblies.ToImmutable();
    }

    private IEnumerable<string> CollectExternalDependencies(IConfig context, Key buildTarget) {
      var allNuGetDependencies = context.GetNuGetResolvedPackages();
      var directExternalDependencies = context.GetExternalDependencies(buildTarget);
      var nuGetRepositoryPath = context.GetNuGetRepositoryDir();
      return CollectDependenciesTransitively(directExternalDependencies.Select(dependency => allNuGetDependencies.GetResolvedNuGetDependency(dependency)),
                                             allNuGetDependencies)
        .Select(dependency => dependency.Assemblies.Select(assemblyReference => assemblyReference.GetAbsolutePath(nuGetRepositoryPath)).First());
    }

    private static IEnumerable<Package> CollectDependenciesTransitively(IEnumerable<Package> directDependencies, NuGetPackages allExternalDependencies) {
      return directDependencies.Select(directDependency => allExternalDependencies.GetResolvedNuGetDependency(directDependency.Id, directDependency.Version))
                               .Concat(directDependencies.SelectMany(directDependency => CollectDependenciesTransitively(directDependency.Dependencies, allExternalDependencies)));
    }

    private static IEnumerable<Package> CollectDependenciesTransitively(ImmutableList<PackageInfo> directDependencies, NuGetPackages allExternalDependencies) {
      return CollectDependenciesTransitively(directDependencies.Select(directDependency => allExternalDependencies.GetResolvedNuGetDependency(directDependency.Id, SemanticVersion.Parse(directDependency.Version))), allExternalDependencies);
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

    private async Task<Unit> CreateDistributablePackage(IContext context, Key buildTarget) {
      await context.Evaluate(BuildKeys.Build.In(buildTarget));
      var referencedAssemblies = await context.CollectCSharpReferencedAssemblies(buildTarget);
      var targetDir = Path.Combine(context.GetOutputDir(buildTarget), "dist");
      Directory.CreateDirectory(targetDir);
      foreach (var referencedAssembly in referencedAssemblies) {
        CopyFile(referencedAssembly, targetDir);
      }
      CopyFile(context.GetCSharpOutputAssemblyFile(buildTarget), targetDir);
      return Unit.Instance;
    }

    private static void CopyFile(string referencedAssembly, string targetDir) {
      File.Copy(referencedAssembly, Path.Combine(targetDir, Path.GetFileName(referencedAssembly)), true);
    }
  }
}