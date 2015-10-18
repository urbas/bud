using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Bud.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static System.IO.Path;
using static Bud.Build;
using static Bud.Conf;

namespace Bud {
  public static class CSharp {
    public static readonly Key<Task> Compile = nameof(Compile);
    public static readonly Key<Assemblies> AssemblyReferences = nameof(AssemblyReferences);
    public static readonly Key<IObservable<CompilationOutput>> Compilation = nameof(Compilation);
    public static readonly Key<Func<CompilationInput, CompilationOutput>> CSharpCompiler = nameof(CSharpCompiler);
    public static readonly Key<string> OutputDir = nameof(OutputDir);
    public static readonly Key<string> AssemblyName = nameof(AssemblyName);
    public static readonly Key<CSharpCompilationOptions> CSharpCompilationOptions = nameof(CSharpCompilationOptions);

    public static Conf CSharpProject(string projectDir, string projectId)
      => Project(projectDir, projectId)
        .Add(SourceDir(fileFilter: "*.cs"))
        .Add(ExcludeSourceDirs("obj", "bin", "target"))
        .Add(CSharpCompilation());

    public static Conf CSharpCompilation()
      => Empty.Init(Compilation, DefaultCompilation)
              .Init(Compile, configs => Compilation[configs].Do(PrintCompilationResult).ToTask())
              .Init(OutputDir, configs => Combine(ProjectDir[configs], "target"))
              .Init(AssemblyName, configs => ProjectId[configs] + CSharpCompilationOptions[configs].OutputKind.ToExtension())
              .Init(AssemblyReferences, configs => new Assemblies(new[] {typeof(object).Assembly.Location}))
              .Init(CSharpCompiler, TimedEmittingCompiler.Create)
              .InitConst(CSharpCompilationOptions, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static void PrintCompilationResult(CompilationOutput output) {
      if (output.Success) {
        Console.WriteLine($"Compiled '{GetFileNameWithoutExtension(output.AssemblyPath)}' in {output.CompilationTime.Milliseconds}ms.");
      } else {
        Console.WriteLine($"Failed to compile '{output.AssemblyPath}'.");
        foreach (var diagnostic in output.Diagnostics) {
          Console.WriteLine(diagnostic);
        }
      }
    }

    private static IObservable<CompilationOutput> DefaultCompilation(IConf conf) {
      var watchedSources = Sources[conf].Watch();
      var watchedAssemblies = AssemblyReferences[conf].Watch();
      return watchedSources.CombineLatest(watchedAssemblies, (files, assemblies) => new {files, assemblies})
                           .Sample(TimeSpan.FromMilliseconds(25))
                           .Select(tuple => CSharpCompiler[conf](new CompilationInput(tuple.files, tuple.assemblies)));
    }
  }
}