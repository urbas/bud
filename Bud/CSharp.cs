using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Bud.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static System.IO.Path;
using static Bud.Build;
using static Bud.Configs;

namespace Bud {
  public static class CSharp {
    public static readonly Key<IObservable<CompilationOutput>> Compilation = nameof(Compilation);
    public static readonly Key<IObservable<IEnumerable<Timestamped<Dependency>>>> Dependencies = nameof(Dependencies);
    public static readonly Key<Func<CompilationInput, CompilationOutput>> CSharpCompiler = nameof(CSharpCompiler);
    public static readonly Key<string> OutputDir = nameof(OutputDir);
    public static readonly Key<string> AssemblyName = nameof(AssemblyName);
    public static readonly Key<CSharpCompilationOptions> CSharpCompilationOptions = nameof(CSharpCompilationOptions);

    public static Configs CSharpProject(string projectDir, string projectId = null) => Project(projectDir, projectId)
      .Add(SourceDir(fileFilter: "*.cs"))
      .Add(ExcludeSourceDirs("obj", "bin", "target"))
      .Add(CSharpCompilation());

    public static Configs CSharpCompilation() => Empty.Init(Compilation, DefaultCompilation)
                                                      .Init(OutputDir, configs => Combine(ProjectDir[configs], "target"))
                                                      .Init(AssemblyName, configs => ProjectId[configs] + CSharpCompilationOptions[configs].OutputKind.ToExtension())
                                                      .Init(Dependencies, configs => FilesObservatory[configs].ObserveAssemblies(typeof(object).Assembly.Location))
                                                      .Init(CSharpCompiler, TimedEmittingCompiler.Create)
                                                      .InitConst(CSharpCompilationOptions, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static IObservable<CompilationOutput> DefaultCompilation(IConfigs configs)
      => Sources[configs].CombineLatest(Dependencies[configs], CompilationInput.Create)
                         .Select(CSharpCompiler[configs]);
  }
}