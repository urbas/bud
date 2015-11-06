using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Bud.Cs;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static System.IO.Path;
using static System.Reactive.Linq.Observable;
using static Bud.Build;

namespace Bud {
  public static class CSharp {
    public static readonly Key<IObservable<CompileOutput>> Compile = nameof(Compile);
    public static readonly Key<IObservable<CompileInput>> CompilationInput = nameof(CompilationInput);
    public static readonly Key<Func<CompileInput, CompileOutput>> Compiler = nameof(Compiler);
    public static readonly Key<Assemblies> AssemblyReferences = nameof(AssemblyReferences);
    public static readonly Key<string> OutputDir = nameof(OutputDir);
    public static readonly Key<string> AssemblyName = nameof(AssemblyName);
    public static readonly Key<CSharpCompilationOptions> CSharpCompilationOptions = nameof(CSharpCompilationOptions);

    public static Conf CSharpProject(string projectId)
      => CSharpProject(projectId, projectId);

    public static Conf CSharpProject(string projectDir, string projectId)
      => Project(projectDir, projectId)
        .Add(SourceDir(fileFilter: "*.cs"))
        .Add(ExcludeSourceDirs("obj", "bin", "target"))
        .Add(CSharpCompilation())
        .In(projectId);

    public static Conf CSharpCompilation()
      => Conf.Empty.Init(Compile, DefaultCompilation)
             .Init(OutputDir, configs => Combine(ProjectDir[configs], "target"))
             .Init(AssemblyName, configs => ProjectId[configs] + CSharpCompilationOptions[configs].OutputKind.ToExtension())
             .Init(AssemblyReferences, conf => new Assemblies(typeof(object).Assembly.Location))
             .Init(Compiler, TimedEmittingCompiler.Create)
             .InitValue(CSharpCompilationOptions, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
             .Init(CompilationInput, DefaultCompilationInput);

    private static void PrintCompilationResult(CompileOutput output) {
      if (output.Success) {
        Console.WriteLine($"Compiled '{GetFileNameWithoutExtension(output.AssemblyPath)}' in {output.CompilationTime.Milliseconds}ms.");
      } else {
        Console.WriteLine($"Failed to compile '{output.AssemblyPath}'.");
        foreach (var diagnostic in output.Diagnostics) {
          Console.WriteLine(diagnostic);
        }
      }
    }

    private static IObservable<CompileOutput> DefaultCompilation(IConf conf)
      => CompilationInput[conf].Select(Compiler[conf])
                               .Do(PrintCompilationResult);

    private static IObservable<CompileInput> DefaultCompilationInput(IConf conf)
      => Observable.CombineLatest(Sources[conf].Watch(),
                                  AssemblyReferences[conf].Watch(),
                                  CollectDependencies(conf),
                                  ToCompilationInput);

    private static IObservable<IEnumerable<CompileOutput>> CollectDependencies(IConf conf)
      => Dependencies[conf].Any() ?
        Dependencies[conf].Select(s => conf.Get(s / Compile)).CombineLatest() :
        Return(Enumerable.Empty<CompileOutput>());

    private static CompileInput ToCompilationInput(IEnumerable<string> files,
                                                   IEnumerable<AssemblyReference> assemblies,
                                                   IEnumerable<CompileOutput> dependencies)
      => new CompileInput(files, assemblies, dependencies);
  }
}