using System;
using System.Reactive.Linq;
using Bud.Compilation;
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
    public static readonly Key<ICSharpCompiler> CSharpCompiler = nameof(CSharpCompiler);
    public static readonly Key<IObservable<ICompilationResult>> Compile = nameof(Compile);
    public static readonly Key<Func<IObservable<IFiles>, IObservable<IFiles>>> SourcesObservationStrategy = nameof(SourcesObservationStrategy);

    public static Tasks CSharpCompilation(ICSharpCompiler compiler = null, Func<IObservable<IFiles>, IObservable<IFiles>> sourceObservationStrategy = null)
      => NewTasks.Init(Compile, PerformCompilation)
                 .InitConst(CSharpCompiler, compiler ?? new RoslynCSharpCompiler())
                 .InitConst(SourcesObservationStrategy, sourceObservationStrategy ?? DefaultSourceObservationStrategy);

    private static IObservable<ICompilationResult> PerformCompilation(ITasks tasks)
      => CSharpCompiler[tasks].Compile(sourceFiles: SourcesObservationStrategy[tasks](SourceFiles[tasks].AsObservable()),
                                       outputDir: Combine(ProjectDir[tasks], "target"),
                                       assemblyName: ProjectId[tasks] + ".dll",
                                       options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                                       references: new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)});

    private static IObservable<IFiles> DefaultSourceObservationStrategy(IObservable<IFiles> sources)
      => sources.Sample(TimeSpan.FromMilliseconds(100)).Delay(TimeSpan.FromMilliseconds(25));
  }
}