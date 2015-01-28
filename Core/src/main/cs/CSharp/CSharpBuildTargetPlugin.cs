using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Build;
using Bud.CSharp.Compiler;
using Bud.Dependencies;
using Bud.Util;
using NuGet;

namespace Bud.CSharp {
  public class CSharpBuildTargetPlugin : BuildTargetPlugin {
    public CSharpBuildTargetPlugin(Key buildScope, params Setup[] setups) : base(buildScope, CSharpKeys.CSharp, setups) {}

    public static Setup Init(Key project, params Setup[] setups) {
      return new CSharpBuildTargetPlugin(project, setups).Setup;
    }

    protected override Settings Setup(Settings settings, Key project) {
      return settings.Do(CSharpKeys.SourceFiles.InitSync(FindSources),
                         CSharpKeys.TargetFramework.Init(Framework.Net45),
                         CSharpKeys.AssemblyType.Init(AssemblyType.Exe),
                         CSharpKeys.CollectReferencedAssemblies.Init(CollectReferencedAssembliesImpl),
                         CSharpKeys.OutputAssemblyDir.Init(GetDefaultOutputAssemblyDir),
                         CSharpKeys.OutputAssemblyName.Init(context => OutputAssemblyName(project, BuildScope)),
                         CSharpKeys.OutputAssemblyFile.Init(GetDefaultOutputAssemblyFile),
                         CSharpKeys.Dist.Init(CreateDistributablePackage));
    }

    protected override Task BuildTaskImpl(IContext context, Key buildTarget) {
      return CSharpCompiler.CompileBuildTarget(context, buildTarget);
    }

    private static string GetDefaultOutputAssemblyDir(IConfig context, Key buildTarget) {
      return Path.Combine(context.GetOutputDir(buildTarget), "debug", "bin");
    }

    private static string OutputAssemblyName(Key project, Key scope) {
      if (BuildKeys.Main.Equals(scope)) {
        return project.Id;
      }
      return project.Id + "." + StringUtils.Capitalize(scope.Id);
    }

    private static string GetDefaultOutputAssemblyFile(IConfig context, Key buildTarget) {
      return Path.Combine(context.GetCSharpOutputAssemblyDir(buildTarget), String.Format("{0}.{1}", context.GetCSharpOutputAssemblyName(buildTarget), GetAssemblyFileExtension(context, buildTarget)));
    }

    private IEnumerable<string> FindSources(IContext context, Key buildTarget) {
      var sourceDirectory = Path.Combine(context.GetBaseDir(buildTarget));
      if (Directory.Exists(sourceDirectory)) {
        return Directory.EnumerateFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories);
      }
      return ImmutableList<string>.Empty;
    }

    private async Task<ImmutableList<string>> CollectReferencedAssembliesImpl(IContext context, Key buildTarget) {
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
      return directDependencies.Select(directDependency => allExternalDependencies.GetResolvedNuGetDependency(directDependency.Id, new VersionSpec(directDependency.Version)))
                               .Concat(directDependencies.SelectMany(directDependency => CollectDependenciesTransitively(directDependency.Dependencies, allExternalDependencies)));
    }

    private static IEnumerable<Package> CollectDependenciesTransitively(ImmutableList<PackageInfo> directDependencies, NuGetPackages allExternalDependencies) {
      return CollectDependenciesTransitively(directDependencies.Select(directDependency => allExternalDependencies.GetResolvedNuGetDependency(directDependency.Id, VersionUtility.ParseVersionSpec(directDependency.Version))), allExternalDependencies);
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

    private async Task CreateDistributablePackage(IContext context, Key buildTarget) {
      await context.Evaluate(BuildKeys.Build.In(buildTarget));
      var referencedAssemblies = await context.CollectCSharpReferencedAssemblies(buildTarget);
      var targetDir = Path.Combine(context.GetOutputDir(buildTarget), "dist");
      Directory.CreateDirectory(targetDir);
      foreach (var referencedAssembly in referencedAssemblies) {
        CopyFile(referencedAssembly, targetDir);
      }
      CopyFile(context.GetCSharpOutputAssemblyFile(buildTarget), targetDir);
    }

    private static void CopyFile(string referencedAssembly, string targetDir) {
      File.Copy(referencedAssembly, Path.Combine(targetDir, Path.GetFileName(referencedAssembly)), true);
    }
  }
}