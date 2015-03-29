using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Build;
using Bud.CSharp.Compiler;
using Bud.Dependencies;
using Bud.IO;
using Bud.Publishing;
using Bud.SolutionExporter;
using NuGet;

namespace Bud.CSharp {
  public class CsBuild : BuildTargetPlugin {
    public const string DistDirName = "dist";

    public CsBuild(Key buildScope, params Setup[] extraBuildTargetSetup) : base(buildScope, CSharpKeys.CSharp, extraBuildTargetSetup) {}

    protected override Settings BuildTargetSetup(Settings buildTargetSettings) {
      return buildTargetSettings.Add(BuildTargetKeys.SourceFiles.InitSync(FindSources),
                                     CSharpKeys.TargetFramework.Init(GetDefaultFramework),
                                     CSharpKeys.AssemblyType.Init(AssemblyType.Exe),
                                     BuildKeys.Build.Modify(BuildTaskImpl),
                                     CSharpKeys.AssemblyReferences.Init(AssemblyReferencesImpl),
                                     CSharpKeys.AssemblyReferencePaths.Init(AssemblyReferencePathsImpl),
                                     CSharpKeys.OutputAssemblyDir.Init(GetDefaultOutputAssemblyDir),
                                     CSharpKeys.OutputAssemblyName.Init(GetDefaultOutputAssemblyName),
                                     CSharpKeys.RootNamespace.Init(GetDefaultRootNamespace),
                                     CSharpKeys.OutputAssemblyFile.Init(GetDefaultOutputAssemblyFile),
                                     CSharpKeys.DistDir.Init(DefaultDistDir),
                                     CSharpKeys.Dist.Init(CreateDistributablePackage),
                                     PublishingPlugin.Instance,
                                     SolutionExporterPlugin.Instance);
    }

    private static Framework GetDefaultFramework(IConfig config) {
      var globalTargetFramework = Key.Root / CSharpKeys.TargetFramework;
      if (config.IsConfigDefined(globalTargetFramework)) {
        return config.Evaluate(globalTargetFramework);
      }
      return Framework.Net45;
    }

    private async Task BuildTaskImpl(IContext context, Func<Task> oldBuild, Key buildTarget) {
      await oldBuild();
      await CSharpCompiler.CompileBuildTarget(context, buildTarget);
    }

    private static string GetDefaultOutputAssemblyDir(IConfig context, Key buildTarget) {
      return Path.Combine(context.GetOutputDir(buildTarget), "debug", "bin");
    }

    private static string GetDefaultOutputAssemblyName(IConfig context, Key buildTarget) {
      return BuildTargetUtils.PackageIdOf(buildTarget);
    }

    private static string GetDefaultRootNamespace(IConfig context, Key buildTarget) {
      var mainCSharpBuildTarget = BuildTargetUtils.ProjectOf(buildTarget) / BuildKeys.Main / CSharpKeys.CSharp;
      return BuildTargetUtils.PackageIdOf(mainCSharpBuildTarget);
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

    private IEnumerable<IPackageAssemblyReference> AssemblyReferencesImpl(IConfig config, Key buildTarget) {
      return config.GetDependencies(buildTarget)
                   .SelectMany(dependency => GetCompatibleAssemblies(config, buildTarget, dependency));
    }

    private static string[] AssemblyReferencePathsImpl(IConfig config, Key buildTarget) {
      return config.GetAssemblyReferences(buildTarget)
                   .Select(assembly => assembly.EffectivePath)
                   .ToArray();
    }

    private IEnumerable<IPackageAssemblyReference> GetCompatibleAssemblies(IConfig config, Key buildTarget, IDependency dependency) {
      IEnumerable<IPackageAssemblyReference> compatibleAssemblyRereferences;
      var package = dependency.AsPackage(config);
      var targetFramework = config.GetTargetFramework(buildTarget).FrameworkName;
      if (VersionUtility.TryGetCompatibleItems(targetFramework, package.AssemblyReferences, out compatibleAssemblyRereferences) && compatibleAssemblyRereferences.Any()) {
        return compatibleAssemblyRereferences.GroupBy(assemblyReference => assemblyReference.Name)
                                             .Select(GetAssemblyWithLatestTargetFramework);
      }
      throw new Exception(String.Format("Could not find a compatible assembly in dependency '{0}' for build target '{1}'. The build target requires target framework of '{2}'.", package.Id, buildTarget, targetFramework.FullName));
    }

    private IPackageAssemblyReference GetAssemblyWithLatestTargetFramework(IEnumerable<IPackageAssemblyReference> compatibleAssemblyRereferences) {
      IPackageAssemblyReference bestAssemblyReference = null;
      foreach (var assemblyReference in compatibleAssemblyRereferences) {
        if (bestAssemblyReference == null || HasNewerTargetFramework(bestAssemblyReference, assemblyReference)) {
          bestAssemblyReference = assemblyReference;
        }
      }
      return bestAssemblyReference;
    }

    private static bool HasNewerTargetFramework(IPackageAssemblyReference relativeToAssemblyReference, IPackageAssemblyReference assemblyReference) {
      return relativeToAssemblyReference.TargetFramework != null &&
             (assemblyReference.TargetFramework == null || (
               assemblyReference.TargetFramework != null && relativeToAssemblyReference.TargetFramework.Version < assemblyReference.TargetFramework.Version));
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

    private string DefaultDistDir(IConfig context, Key buildTarget) {
      return Path.Combine(context.GetOutputDir(buildTarget), DistDirName);
    }

    private async Task CreateDistributablePackage(IContext context, Key buildTarget) {
      await context.Evaluate(buildTarget / BuildKeys.Build);
      var referencedAssemblies = context.GetAssemblyReferencePaths(buildTarget);
      var distributionPath = context.GetDistDir(buildTarget);
      Directory.CreateDirectory(distributionPath);
      foreach (var referencedAssembly in referencedAssemblies) {
        Files.CopyFile(referencedAssembly, distributionPath);
      }
      Files.CopyFile(context.GetCSharpOutputAssemblyFile(buildTarget), distributionPath);
    }

    public static bool IsMainBuildTargetDefined(IConfig context, Key dependencyBuildTargetKey) {
      return context.IsConfigDefined(dependencyBuildTargetKey / CSharpKeys.OutputAssemblyFile);
    }
  }
}