using System;
using System.Collections.Generic;
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
    public static readonly Key<IObservable<ICompilationResult>> Compile = nameof(Compile);
    public static readonly Key<string> OutputDir = nameof(OutputDir);
    public static readonly Key<string> AssemblyName = nameof(AssemblyName);
    public static readonly Key<IEnumerable<MetadataReference>> References = nameof(References);
    public static readonly Key<CSharpCompilationOptions> CSharpCompilationOptions = nameof(CSharpCompilationOptions);
    public static readonly Key<ICSharpCompiler> CSharpCompiler = nameof(CSharpCompiler);
    public static readonly Key<Func<IObservable<FilesUpdate>, IObservable<FilesUpdate>>> SourcesObservationStrategy = nameof(SourcesObservationStrategy);

    public static Configs CSharpCompilation()
      => Empty.Init(Compile, PerformCompilation)
              .Init(OutputDir, configs => Combine(ProjectDir[configs], "target"))
              .Init(AssemblyName, configs => ProjectId[configs] + CSharpCompilationOptions[configs].OutputKind.ToExtension())
              .Init(References, configs => new[] {MetadataReference.CreateFromFile(typeof(object).Assembly.Location)})
              .InitConst(CSharpCompiler, new RoslynCSharpCompiler())
              .InitConst(CSharpCompilationOptions, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
              .InitConst(SourcesObservationStrategy, DefaultSourceObservationStrategy);

    private static IObservable<ICompilationResult> PerformCompilation(IConfigs configs)
      => CSharpCompiler[configs].Compile(SourcesObservationStrategy[configs](Sources[configs].Watch()),
                                         OutputDir[configs],
                                         AssemblyName[configs],
                                         CSharpCompilationOptions[configs],
                                         References[configs]);

    private static IObservable<FilesUpdate> DefaultSourceObservationStrategy(IObservable<FilesUpdate> sources)
      => sources.Sample(TimeSpan.FromMilliseconds(100))
                .Delay(TimeSpan.FromMilliseconds(25));

    public static string ToExtension(this OutputKind kind) {
      switch (kind) {
        case OutputKind.ConsoleApplication:
        case OutputKind.WindowsApplication:
        case OutputKind.WindowsRuntimeApplication:
          return ".exe";
        case OutputKind.DynamicallyLinkedLibrary:
          return ".dll";
        case OutputKind.NetModule:
          return ".netmodule";
        case OutputKind.WindowsRuntimeMetadata:
          return ".winmdobj";
        default:
          return ".dll";
      }
    }
  }
}