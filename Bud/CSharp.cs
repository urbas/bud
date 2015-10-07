using System;
using System.Reactive.Linq;
using Bud.Compilation;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static System.IO.Path;
using static Bud.Build;
using static Bud.Configs;

namespace Bud {
  public static class CSharp {
    public static readonly Key<ICSharpCompiler> CSharpCompiler = nameof(CSharpCompiler);
    public static readonly Key<IObservable<ICompilationResult>> Compile = nameof(Compile);
    public static readonly Key<Func<IObservable<IFiles>, IObservable<IFiles>>> SourcesObservationStrategy = nameof(SourcesObservationStrategy);

    public static Configs CSharpCompilation(ICSharpCompiler compiler = null, Func<IObservable<IFiles>, IObservable<IFiles>> sourceObservationStrategy = null)
      => NewConfigs.Init(Compile, PerformCompilation)
                 .InitConst(CSharpCompiler, compiler ?? new RoslynCSharpCompiler())
                 .InitConst(SourcesObservationStrategy, sourceObservationStrategy ?? DefaultSourceObservationStrategy);

    private static IObservable<ICompilationResult> PerformCompilation(IConfigs configs)
      => CSharpCompiler[configs].Compile(sourceFiles: SourcesObservationStrategy[configs](Sources[configs].AsObservable()),
                                       outputDir: Combine(ProjectDir[configs], "target"),
                                       assemblyName: ProjectId[configs] + ".dll",
                                       options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                                       references: new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)});

    private static IObservable<IFiles> DefaultSourceObservationStrategy(IObservable<IFiles> sources)
      => sources.Sample(TimeSpan.FromMilliseconds(100))
                .Delay(TimeSpan.FromMilliseconds(25));
  }
}