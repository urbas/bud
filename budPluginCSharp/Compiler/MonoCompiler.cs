using System.Threading.Tasks;
using Bud.Plugins.Projects;
using System.IO;
using Bud.Cli;
using System;
using System.Linq;

namespace Bud.Plugins.CSharp.Compiler {
  public static class MonoCompiler {

    public async static Task<Unit> CompileAllProjects(EvaluationContext context) {
      foreach (var project in context.Evaluate(ProjectKeys.ListOfProjects)) {
        await context.Evaluate(CSharpPlugin.CSharpBuild.In(project));
      }
      return Unit.Instance;
    }

    public static Task<Unit> CompileProject(EvaluationContext context, Scope project) {
      var sourceDirectory = context.GetCSharpSourceDir(project);
      var outputFile = context.GetCSharpOutputAssemblyFile(project);
      return Task.Run(() => {
        var sourceFiles = Directory.EnumerateFiles(sourceDirectory);
        if (Directory.Exists(sourceDirectory) && sourceFiles.Any()) {
          Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
          var cSharpCompiler = "/usr/bin/mcs";
          var exitCode = ProcessBuilder.Executable(cSharpCompiler).WithArgument("-out:" + outputFile).WithArguments(sourceFiles).Start(Console.Out, Console.Error);
          if (exitCode != 0) {
            throw new Exception("Compilation failed.");
          }
        }
        return Unit.Instance;
      });
    }
  }
}

