using System;
using System.Linq;
using System.Reactive.Linq;
using Bud.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static System.IO.Path;
using static Bud.Build;
using static Bud.Conf;
using static Bud.Keys;

namespace Bud {
  public static class CSharp {
    public static readonly Key<Assemblies> AssemblyReferences = nameof(AssemblyReferences);
    public static readonly Key<IObservable<CompilationOutput>> Compile = nameof(Compile);
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
      => Empty.Init(Compile, DefaultCompilation)
              .Init(OutputDir, configs => Combine(ProjectDir[configs], "target"))
              .Init(AssemblyName, configs => ProjectId[configs] + CSharpCompilationOptions[configs].OutputKind.ToExtension())
              .Init(AssemblyReferences, DefaultAssemblyReferences)
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
                           .Select(tuple => CSharpCompiler[conf](new CompilationInput(tuple.files, tuple.assemblies)))
                           .Do(PrintCompilationResult);
    }

    private static Assemblies DefaultAssemblyReferences(IConf configs) {
      var dependencyAssemblies = Dependencies[configs]
        .Select(dependency => configs.Get(Root / dependency / Compile).Take(1).Wait());
      return new Assemblies(typeof(object).Assembly.Location)
        .ExpandWith(new Assemblies(dependencyAssemblies.Select(output => output.ToAssemblyReference())));
    }
  }
}