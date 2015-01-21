using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Cli;

namespace Bud.Plugins.CSharp.Compiler {
  public static class CSharpCompiler {
    public static Task<Unit> CompileProject(IContext context, Key buildKey) {
      return Task.Run(async () => {
        var outputFile = context.GetCSharpOutputAssemblyFile(buildKey);
        var framework = context.GetTargetFramework(buildKey);
        var sourceFiles = await context.GetCSharpSources(buildKey);
        var libraryDependencies = await GetReferencedAssemblies(context, buildKey);
        var frameworkAssemblies = framework.RuntimeAssemblies;
        if (sourceFiles.Any()) {
          Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
          var compilerProcess = ProcessBuilder
            .Executable(framework.CSharpCompilerPath)
            .AddParamArgument("-out:", outputFile);
          if (libraryDependencies.Any()) {
            compilerProcess = compilerProcess.AddParamArgument("-reference:", libraryDependencies.Concat(frameworkAssemblies));
          }
          compilerProcess = compilerProcess.AddParamArgument("-target:", GetTargetKind(context.GetCSharpAssemblyType(buildKey)));
          compilerProcess = compilerProcess.AddArguments(sourceFiles);
          var exitCode = compilerProcess.Start(Console.Out, Console.Error);
          if (exitCode != 0) {
            throw new Exception("Compilation failed.");
          }
        }
        return Unit.Instance;
      });
    }

    private static async Task<IEnumerable<string>> GetReferencedAssemblies(IContext context, Key buildKey) {
      return await context.CollectCSharpReferencedAssemblies(buildKey);
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