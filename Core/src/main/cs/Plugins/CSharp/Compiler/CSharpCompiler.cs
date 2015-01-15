using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Cli;

namespace Bud.Plugins.CSharp.Compiler {
  public static class CSharpCompiler {
    private const string WindowsCompilerExecutablePath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe";
    private const string UnixCompilerExecutable = "/usr/bin/mcs";
    private static readonly string[] UnixSystemRuntimeFacadeDll = {"Facades/System.Runtime.dll"};
    private static readonly string[] WindowsSystemRuntimeFacadeDll = {"System.Runtime.dll"};

    public static Task<Unit> CompileProject(IContext context, Key buildKey) {
      return Task.Run(async () => {
        var outputFile = context.GetCSharpOutputAssemblyFile(buildKey);
        var sourceFiles = await context.GetCSharpSources(buildKey);
        var libraryDependencies = await GetReferencedAssemblies(context, buildKey);
        if (sourceFiles.Any()) {
          Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
          var compilerProcess = ProcessBuilder
            .Executable(GetCompilerExecutablePath())
            .AddParamArgument("-out:", outputFile);
          if (libraryDependencies.Any()) {
            compilerProcess = compilerProcess.AddParamArgument("-reference:", libraryDependencies);
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
      var configuredDependencies = await context.CollectCSharpReferencedAssemblies(buildKey);
      return configuredDependencies.Concat(GetPlatformSpecificAssemblies());
    }

    private static IEnumerable<string> GetPlatformSpecificAssemblies() {
      if (Environment.OSVersion.Platform == PlatformID.Unix) {
        return UnixSystemRuntimeFacadeDll;
      }
      return WindowsSystemRuntimeFacadeDll;
    }

    private static string GetCompilerExecutablePath() {
      if (Environment.OSVersion.Platform == PlatformID.Unix) {
        return UnixCompilerExecutable;
      } else {
        return WindowsCompilerExecutablePath;
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