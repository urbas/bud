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
    public static readonly Key<IObservable<CSharpCompilation>> Compilation = nameof(Compilation);
    public static readonly Key<string> OutputDir = nameof(OutputDir);
    public static readonly Key<string> AssemblyName = nameof(AssemblyName);
    public static readonly Key<IObservable<IEnumerable<Timestamped<Dependency>>>> Dependencies = nameof(Dependencies);
    public static readonly Key<CSharpCompilationOptions> CSharpCompilationOptions = nameof(CSharpCompilationOptions);
    public static readonly Key<ICSharpCompiler> CSharpCompiler = nameof(CSharpCompiler);
    public static readonly Key<Func<IObservable<IEnumerable<string>>, IObservable<IEnumerable<string>>>> SourcesObservationStrategy = nameof(SourcesObservationStrategy);

    public static Configs CSharpCompilation()
      => Empty.Init(Compilation, configs => CSharpCompiler[configs].Compile(SourcesObservationStrategy[configs](Sources[configs]).CombineLatest(Dependencies[configs], (enumerable, references) => new CSharpCompilationInput(enumerable, references)), configs))
              .Init(OutputDir, configs => Combine(ProjectDir[configs], "target"))
              .Init(AssemblyName, configs => ProjectId[configs] + CSharpCompilationOptions[configs].OutputKind.ToExtension())
              .Init(Dependencies, configs => FilesObservatory[configs].ObserveAssemblies(typeof(object).Assembly.Location))
              .InitConst(CSharpCompiler, new RoslynCSharpCompiler())
              .InitConst(CSharpCompilationOptions, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
              .InitConst(SourcesObservationStrategy, DefaultSourceObservationStrategy);

    private static IObservable<T> DefaultSourceObservationStrategy<T>(IObservable<T> sources)
      => sources.Sample(TimeSpan.FromMilliseconds(100)).Delay(TimeSpan.FromMilliseconds(25));

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

  public class Dependency {
    public Dependency(string path, MetadataReference metadataReference) {
      Path = path;
      MetadataReference = metadataReference;
    }

    public string Path { get; }
    public MetadataReference MetadataReference { get; }

    public override string ToString() => Path;

    public bool Equals(Dependency other) => string.Equals(Path, other.Path);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == typeof(Dependency) && Equals((Dependency) obj);
    }

    public override int GetHashCode() => Path.GetHashCode();

    public static bool operator ==(Dependency left, Dependency right) => Equals(left, right);

    public static bool operator !=(Dependency left, Dependency right) => !Equals(left, right);
  }
}