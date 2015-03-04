using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Cli;
using Bud.Dependencies;
using Bud.IO;

namespace Bud.CSharp.Compiler {
  public static class CSharpCompiler {
    public static Task CompileBuildTarget(IContext context, Key buildTarget) {
      return Task.Run(async () => {
        await context.EvaluateInternalDependencies(buildTarget);
        var outputFile = context.GetCSharpOutputAssemblyFile(buildTarget);
        var framework = context.GetTargetFramework(buildTarget);
        var sourceFiles = await context.GetCSharpSources(buildTarget);
        var libraryDependencies = context.GetReferencedAssemblyPaths(buildTarget);
        var frameworkAssemblies = framework.RuntimeAssemblies;
        if (sourceFiles.Any() && Files.AreFilesNewer(sourceFiles, outputFile)) {
          Compile(context, buildTarget, outputFile, framework, libraryDependencies, frameworkAssemblies, sourceFiles);
        } else {
          context.Logger.Info("skipping build...");
        }
      });
    }

    private static void Compile(IContext context, Key buildTarget, string outputFile, Framework framework, IEnumerable<string> libraryDependencies, IEnumerable<string> frameworkAssemblies, IEnumerable<string> sourceFiles) {
      Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
      var compilerProcess = ProcessBuilder.Executable(framework.CSharpCompilerPath)
                                          .AddParamArgument("-out:", outputFile);
      if (libraryDependencies.Any() || frameworkAssemblies.Any()) {
        compilerProcess.AddParamArgument("-reference:", libraryDependencies.Concat(frameworkAssemblies));
      }
      compilerProcess.AddParamArgument("-target:", GetTargetKind(context.GetCSharpAssemblyType(buildTarget)))
                     .AddArguments(sourceFiles);
      var exitCode = compilerProcess.Start(Console.Out, Console.Error);
      if (exitCode != 0) {
        throw new Exception("Compilation failed.");
      }
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