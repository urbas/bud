using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Bud.Compilation;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static System.IO.Path;
using static Bud.Build;
using static Bud.Conf;

namespace Bud {
  public static class CSharp {
    public static readonly Key<IObservable<CSharpCompilationOutput>> Compile = nameof(Compile);
    public static readonly Key<IObservable<CSharpCompilationInput>> CompilationInput = nameof(CompilationInput);
    public static readonly Key<Func<CSharpCompilationInput, CSharpCompilationOutput>> Compiler = nameof(Compiler);
    public static readonly Key<Assemblies> AssemblyReferences = nameof(AssemblyReferences);
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
              .Init(AssemblyReferences, conf => new Assemblies(typeof(object).Assembly.Location))
              .Init(Compiler, TimedEmittingCompiler.Create)
              .InitConst(CSharpCompilationOptions, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
              .Init(CompilationInput, DefaultCompilationInput);

    private static void PrintCompilationResult(CSharpCompilationOutput output) {
      if (output.Success) {
        Console.WriteLine($"Compiled '{GetFileNameWithoutExtension(output.AssemblyPath)}' in {output.CompilationTime.Milliseconds}ms.");
      } else {
        Console.WriteLine($"Failed to compile '{output.AssemblyPath}'.");
        foreach (var diagnostic in output.Diagnostics) {
          Console.WriteLine(diagnostic);
        }
      }
    }

    private static IObservable<CSharpCompilationOutput> DefaultCompilation(IConf conf)
      => CompilationInput[conf].Select(Compiler[conf])
                               .Do(PrintCompilationResult);

    private static IObservable<CSharpCompilationInput> DefaultCompilationInput(IConf conf)
      => DefaultCompilationInput(conf, (files, assemblies, cSharpCompilationOutputs) => ToCompilationInput(files, assemblies, cSharpCompilationOutputs));

    internal static IObservable<CSharpCompilationInput> DefaultCompilationInput(IConf conf,
                                                                                Func<Files, Assemblies, IEnumerable<CSharpCompilationOutput>, CSharpCompilationInput> compilationInputBuilder)
      => Observable.CombineLatest(Sources[conf].Watch(),
                                  AssemblyReferences[conf].Watch(),
                                  CollectDependencies(conf),
                                  compilationInputBuilder);

    private static IObservable<IEnumerable<CSharpCompilationOutput>> CollectDependencies(IConf conf)
      => Dependencies[conf].Any() ?
        Dependencies[conf].Select(s => conf.Get(Keys.Root / s / Compile)).CombineLatest() :
        Observable.Return(Enumerable.Empty<CSharpCompilationOutput>());

    private static CSharpCompilationInput ToCompilationInput(Files files,
                                                             Assemblies assemblies,
                                                             IEnumerable<CSharpCompilationOutput> cSharpCompilationOutputs)
      => new CSharpCompilationInput(files, assemblies, cSharpCompilationOutputs);
  }
}