using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Bud.IO;
using Bud.Tasking;
using Bud.Tasking.ApiV1;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static System.IO.Path;
using static Bud.Build;
using static Bud.Tasking.ApiV1.Tasks;

namespace Bud {
  public static class CSharp {
    public static readonly Key<IObservable<CompilationResult>> Compile = "compile";

    public static Tasks RoslynCompiler()
      => NewTasks.Set(Compile, PerformCompilation);

    private static async Task<IObservable<CompilationResult>> PerformCompilation(ITasks tasks) {
      return (await SourceFiles[tasks])
        .AsObservable()
        .Sample(TimeSpan.FromMilliseconds(100))
        .Delay(TimeSpan.FromMilliseconds(25))
        .CombineLatest(ProjectId[tasks].ToObservable(), ProjectDir[tasks].ToObservable(), DoCompile);
    }

    private static CompilationResult DoCompile(IFiles sources, string projectId, string projectDir) {
      var assemblyName = projectId + ".dll";
      var compilation = CSharpCompilation
        .Create(assemblyName, sources.Select(s => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(s))),
                options: new CSharpCompilationOptions(OutputKind.ConsoleApplication),
                references: new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)});
      var targetDir = Combine(projectDir, "target");
      Directory.CreateDirectory(targetDir);
      var assemblyPath = Combine(targetDir, assemblyName);
      using (var assemblyOutputFile = File.Create(assemblyPath)) {
        var emitResult = compilation.Emit(assemblyOutputFile);
        return new CompilationResult(assemblyPath, emitResult);
      }
    }
  }
}