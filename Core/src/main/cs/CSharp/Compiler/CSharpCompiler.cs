using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Cli;
using Bud.Dependencies;

namespace Bud.CSharp.Compiler {
  public static class CSharpCompiler {
    public static Task CompileBuildTarget(IContext context, Key buildTarget) {
      return Task.Run(async () => {
        await context.EvaluateInternalDependencies(buildTarget);
        var outputFile = context.GetCSharpOutputAssemblyFile(buildTarget);
        var framework = context.GetTargetFramework(buildTarget);
        var sourceFiles = await context.GetCSharpSources(buildTarget);
        var libraryDependencies = context.GetReferencedAssemblies(buildTarget);
        var frameworkAssemblies = framework.RuntimeAssemblies;
        if (sourceFiles.Any()) {
          Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
          var compilerProcess = ProcessBuilder
            .Executable(framework.CSharpCompilerPath)
            .AddParamArgument("-out:", outputFile);
          if (libraryDependencies.Any()) {
            compilerProcess = compilerProcess.AddParamArgument("-reference:", libraryDependencies.Concat(frameworkAssemblies));
          }
          compilerProcess = compilerProcess.AddParamArgument("-target:", GetTargetKind(context.GetCSharpAssemblyType(buildTarget)));
          compilerProcess = compilerProcess.AddArguments(sourceFiles);
          var exitCode = compilerProcess.Start(Console.Out, Console.Error);
          if (exitCode != 0) {
            throw new Exception("Compilation failed.");
          }
        }
      });
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