using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bud.Tasking;
using Bud.Tasking.ApiV1;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static System.IO.Path;
using static Bud.Build;
using static Bud.Tasking.ApiV1.Tasks;

namespace Bud {
  public static class CSharp {
    public static readonly Key<CompilationResult> Compile = "compile";

    public static Tasks RoslynCompiler()
      => NewTasks.Set(Compile, PerformCompilation);

    private static async Task<CompilationResult> PerformCompilation(ITasks tasks) {
      var sources = await SourceFiles[tasks];
      var assemblyName = await ProjectId[tasks] + ".dll";
      var compilation = CSharpCompilation
        .Create(assemblyName, sources.Select(s => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(s))),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                references: new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)});
      var targetDir = Combine(await ProjectDir[tasks], "target");
      Directory.CreateDirectory(targetDir);
      var assemblyPath = Combine(targetDir, assemblyName);
      using (var assemblyOutputFile = File.Create(assemblyPath)) {
        var emitResult = compilation.Emit(assemblyOutputFile);
        return new CompilationResult(assemblyPath, emitResult);
      }
    }
  }
}