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

    public CsBuild(Key buildScope, params Setup[] extraBuildTargetSetup) : base(buildScope, Cs.CSharp, extraBuildTargetSetup) {}

    protected override Settings BuildTargetSetup(Settings buildTargetSettings) {
      return buildTargetSettings.Add(BuildTargetKeys.SourceFiles.InitSync(FindSources),
                                     Cs.TargetFramework.Init(GetDefaultFramework),
                                     Cs.AssemblyType.Init(AssemblyType.Exe),
                                     BuildKeys.Build.Modify(BuildTaskImpl),
                                     Cs.AssemblyReferences.Init(AssemblyReferencesImpl),
                                     Cs.AssemblyReferencePaths.Init(AssemblyReferencePathsImpl),
                                     Cs.OutputAssemblyDir.Init(GetDefaultOutputAssemblyDir),
                                     Cs.OutputAssemblyName.Init(GetDefaultOutputAssemblyName),
                                     Cs.RootNamespace.Init(GetDefaultRootNamespace),
                                     Cs.OutputAssemblyFile.Init(GetDefaultOutputAssemblyFile),
                                     Cs.DistDir.Init(DefaultDistDir),
                                     Cs.Dist.Init(CreateDistributablePackage),
                                     PublishingPlugin.Instance,
                                     SolutionExporterPlugin.Instance);
    }

    private static Framework GetDefaultFramework(IConfig config) {
      var globalTargetFramework = Key.Root / Cs.TargetFramework;
      if (config.IsConfigDefined(globalTargetFramework)) {
        return config.Evaluate(globalTargetFramework);
      }
      return Framework.Net45;
    }

    private static async Task BuildTaskImpl(IContext context, Func<Task> oldBuild, Key buildTarget) {
      await oldBuild();
      await CSharpCompiler.CompileBuildTarget(context, buildTarget);
    }

    private static string GetDefaultOutputAssemblyDir(IConfig context, Key buildTarget) {
      return Path.Combine(context.GetOutputDir(buildTarget), "debug", "bin");
    }

    private static string GetDefaultOutputAssemblyName(IConfig context, Key buildTarget) {
      return context.PackageIdOf(buildTarget);
    }

    private static string GetDefaultRootNamespace(IConfig context, Key buildTarget) {
      var mainCSharpBuildTarget = BuildTargetUtils.ProjectOf(buildTarget) / BuildKeys.Main / Cs.CSharp;
      return context.PackageIdOf(mainCSharpBuildTarget);
    }

    private static string GetDefaultOutputAssemblyFile(IConfig context, Key buildTarget) {
      var assemblyDir = context.GetCSharpOutputAssemblyDir(buildTarget);
      var assemblyName = context.GetCSharpOutputAssemblyName(buildTarget);
      var assemblyExtension = GetAssemblyFileExtension(context, buildTarget);
      return Path.Combine(assemblyDir, string.Format("{0}.{1}", assemblyName, assemblyExtension));
    }


    private static IEnumerable<string> FindSources(IContext context, Key buildTarget) {
      var sourceDirectory = context.GetBaseDir(buildTarget);
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

    private static IPackageAssemblyReference GetAssemblyWithLatestTargetFramework(IEnumerable<IPackageAssemblyReference> compatibleAssemblyRereferences) {
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

    private static string DefaultDistDir(IConfig context, Key buildTarget) {
      return Path.Combine(context.GetOutputDir(buildTarget), DistDirName);
    }

    private static async Task CreateDistributablePackage(IContext context, Key buildTarget) {
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
      return context.IsConfigDefined(dependencyBuildTargetKey / Cs.OutputAssemblyFile);
    }
  }
}