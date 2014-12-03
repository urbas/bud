using System.Threading.Tasks;
using Bud.Plugins.Projects;
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
        if (sourceFiles.Any()) {
          Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
          var exitCode = ProcessBuilder.Executable(CSharpCompiler).WithArgument("-out:" + outputFile).WithArguments(sourceFiles).Start(Console.Out, Console.Error);
          if (exitCode != 0) {
            throw new Exception("Compilation failed.");
          }
        }
        return Unit.Instance;
      });
    }
  }
}

