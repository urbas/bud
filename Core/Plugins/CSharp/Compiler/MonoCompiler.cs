using System.Threading.Tasks;
using Bud.Plugins.Projects;
using Bud.Plugins.Build;
using System.IO;
using Bud.Cli;
using System;
using System.Linq;

namespace Bud.Plugins.CSharp.Compiler {
  public static class MonoCompiler {
    public static readonly string CSharpCompiler = "/usr/bin/mcs";

    public static Task<Unit> CompileProject(EvaluationContext context, Scope project) {
      return Task.Run(async () => {
        var outputFile = context.GetCSharpOutputAssemblyFile(project);
        var sourceFiles = await context.GetCSharpSources(project);
        var libraryDependencies = await context.CollectCSharpReferencedAssemblies(project);
        if (sourceFiles.Any()) {
          Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
          var monoCompilerProcess = ProcessBuilder
            .Executable(CSharpCompiler)
            .AddParamArgument("-out:", outputFile);
          if (libraryDependencies.Any()) {
            monoCompilerProcess = monoCompilerProcess.AddParamArgument("-reference:", libraryDependencies);
          }
          monoCompilerProcess = monoCompilerProcess.AddParamArgument("-target:", GetTargetKind(context.GetCSharpAssemblyType(project)));
          monoCompilerProcess = monoCompilerProcess.AddArguments(sourceFiles);
          var exitCode = monoCompilerProcess.Start(Console.Out, Console.Error);
          if (exitCode != 0) {
            throw new Exception("Compilation failed.");
          }
        }
        return Unit.Instance;
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

