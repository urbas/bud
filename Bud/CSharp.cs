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
using static Bud.Configs;

namespace Bud {
  public static class CSharp {
    public static readonly Key<IObservable<CSharpCompilation>> Compilation = nameof(Compilation);
    public static readonly Key<string> OutputDir = nameof(OutputDir);
    public static readonly Key<string> AssemblyName = nameof(AssemblyName);
    public static readonly Key<IReferences> References = nameof(References);
    public static readonly Key<CSharpCompilationOptions> CSharpCompilationOptions = nameof(CSharpCompilationOptions);
    public static readonly Key<ICSharpCompiler> CSharpCompiler = nameof(CSharpCompiler);
    public static readonly Key<Func<IObservable<IEnumerable<string>>, IObservable<IEnumerable<string>>>> SourcesObservationStrategy = nameof(SourcesObservationStrategy);

    public static Configs CSharpCompilation()
      => Empty.Init(Compilation, DefaultCompilation)
              .Init(OutputDir, configs => Combine(ProjectDir[configs], "target"))
              .Init(AssemblyName, configs => ProjectId[configs] + CSharpCompilationOptions[configs].OutputKind.ToExtension())
              .Init(References, configs => Bud.Compilation.References.FromFiles(typeof(object).Assembly.Location))
              .InitConst(CSharpCompiler, new RoslynCSharpCompiler())
              .InitConst(CSharpCompilationOptions, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
              .InitConst(SourcesObservationStrategy, DefaultSourceObservationStrategy);

    private static IObservable<CSharpCompilation> DefaultCompilation(IConfigs configs)
      => CSharpCompiler[configs].Compile(SourcesObservationStrategy[configs](Sources[configs]),
                                         References[configs].Watch(), AssemblyName[configs], CSharpCompilationOptions[configs]);

    private static IObservable<T> DefaultSourceObservationStrategy<T>(IObservable<T> sources)
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

namespace Bud.Compilation {
  public interface IReferences {
    IObservable<IEnumerable<MetadataReference>> Watch();
    IEnumerable<MetadataReference> Enumerate();
  }

  public static class References {
    public static IReferences FromFiles(params string[] locations) => new FileReferences(locations);
    public static IReferences ExtendWith(this IReferences references, IReferences otherReferences) => new CompoundReferences(references, otherReferences);
    public static IReferences ExtendWith(this IReferences references, IObservable<MetadataReference> reference) => new CompoundReferences(references, new ObservableReference(reference));
  }

  public class ObservableReference : IReferences {
    public IObservable<MetadataReference> Reference { get; }

    public ObservableReference(IObservable<MetadataReference> reference) {
      Reference = reference;
    }

    public IObservable<IEnumerable<MetadataReference>> Watch() => Reference.Select(reference => new [] { reference });
    public IEnumerable<MetadataReference> Enumerate() => new[] {Reference.ToEnumerable().First()};
  }

  public class CompoundReferences : IReferences {
    public IReferences References { get; }
    public IReferences OtherReferences { get; }

    public CompoundReferences(IReferences references, IReferences otherReferences) {
      References = references;
      OtherReferences = otherReferences;
    }

    public IObservable<IEnumerable<MetadataReference>> Watch() => References.Watch().Merge(OtherReferences.Watch()).Select(references => Enumerate());
    public IEnumerable<MetadataReference> Enumerate() => References.Enumerate().Concat(OtherReferences.Enumerate());
  }


  public class FileReferences : IReferences {
    public IEnumerable<string> Locations { get; }

    public FileReferences(IEnumerable<string> locations) {
      Locations = locations;
    }

    public IObservable<IEnumerable<MetadataReference>> Watch() => Observable.Return(Enumerate());
    public IEnumerable<MetadataReference> Enumerate() => Locations.Select(s => MetadataReference.CreateFromFile(s));
  }
}