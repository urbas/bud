using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Build;
using Bud.Cli;
using Bud.Dependencies;
using Bud.IO;
using Bud.Resources;
using static System.IO.Path;

namespace Bud.CSharp.Compiler {
  public static class CSharpCompiler {
    public static Task CompileBuildTarget(IContext context, Key buildTarget) {
      return Task.Run(async () => {
        await context.EvaluateInternalDependencies(buildTarget);
        var outputFile = context.GetCSharpOutputAssemblyFile(buildTarget);
        var framework = context.GetTargetFramework(buildTarget);
        var sourceFiles = await context.GetSourceFiles(buildTarget);
        var resourceFiles = await CollectResourceFiles(context, buildTarget);
        var libraryDependencies = context.GetAssemblyReferencePaths(buildTarget);
        var frameworkAssemblies = framework.RuntimeAssemblies;
        if (AreSourceFilesNewer(sourceFiles.Concat(resourceFiles), outputFile)) {
          Compile(context, buildTarget, outputFile, framework, libraryDependencies, frameworkAssemblies, sourceFiles, resourceFiles);
        } else {
          context.Logger.Info("skipping build...");
        }
      });
    }

    private static bool AreSourceFilesNewer(IEnumerable<string> sourceFiles, string outputFile) {
      return sourceFiles.Any() && Files.AreFilesNewer(sourceFiles, outputFile);
    }

    private static Task<IEnumerable<string>> CollectResourceFiles(IContext context, Key buildTarget) {
      var resourceBuildTarget = BuildTargetUtils.ScopeOf(buildTarget) / ResourcesBuildTarget.Resources;
      return context.GetSourceFiles(resourceBuildTarget);
    }

    private static void Compile(IContext context, Key buildTarget, string outputFile, Framework framework, IEnumerable<string> libraryDependencies, IEnumerable<string> frameworkAssemblies, IEnumerable<string> sourceFiles, IEnumerable<string> resourceFiles) {
      Directory.CreateDirectory(GetDirectoryName(outputFile));
      ProcessBuilder.Executable(framework.CSharpCompilerPath)
                    .AddParamArgument("-out:", outputFile)
                    .AddReferences(libraryDependencies, frameworkAssemblies)
                    .AddResourceFiles(context.GetRootNamespace(buildTarget), resourceFiles)
                    .AddParamArgument("-target:", GetTargetKind(context.GetCSharpAssemblyType(buildTarget)))
                    .AddArguments(sourceFiles)
                    .InvokeCompiler();
    }

    private static void InvokeCompiler(this ProcessBuilder compilerProcess) {
      var exitCode = compilerProcess.Start(Console.Out, Console.Error);
      if (exitCode != 0) {
        throw new Exception("Compilation failed.");
      }
    }

    private static ProcessBuilder AddReferences(this ProcessBuilder compilerProcess, IEnumerable<string> libraryDependencies, IEnumerable<string> frameworkAssemblies) {
      if (libraryDependencies.Any() || frameworkAssemblies.Any()) {
        compilerProcess.AddParamArgument("-reference:", libraryDependencies.Concat(frameworkAssemblies));
      }
      return compilerProcess;
    }

    private static ProcessBuilder AddResourceFiles(this ProcessBuilder compilerProcess, string rootNamespace, IEnumerable<string> resourceFiles) {
      foreach (var resourceFile in resourceFiles) {
        string resourceIdentifier = $"{rootNamespace}.{GetFileName(resourceFile)}";
        compilerProcess.AddParamArgument("-resource:", resourceFile, resourceIdentifier);
      }
      return compilerProcess;
    }

    public static string GetTargetKind(AssemblyType assemblyType) {
      switch (assemblyType) {
        case AssemblyType.Exe:
          return "exe";
        case AssemblyType.Library:
          return "library";
        case AssemblyType.WinExe:
          return "winexe";
        case AssemblyType.Module:
          return "module";
        default:
          throw new ArgumentException("Unsupported assembly type.");
      }
    }
  }
}