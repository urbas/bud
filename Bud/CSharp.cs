using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.Cs;
using Bud.IO;
using Bud.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static System.IO.Path;
using static System.Reactive.Linq.Observable;
using static Bud.Builds;

namespace Bud {
  public static class CSharp {
    public static readonly Key<IObservable<CompileOutput>> Compile = nameof(Compile);
    public static readonly Key<Func<InOut, CompileOutput>> Compiler = nameof(Compiler);
    public static readonly Key<IImmutableList<string>> AssemblyReferences = nameof(AssemblyReferences);
    public static readonly Key<string> AssemblyName = nameof(AssemblyName);
    public static readonly Key<CSharpCompilationOptions> CSharpCompilationOptions = nameof(CSharpCompilationOptions);
    public static readonly Key<ImmutableList<ResourceDescription>> EmbeddedResources = nameof(EmbeddedResources);
    public static readonly Key<IImmutableSet<Package>> PackageDependencies = nameof(PackageDependencies);
    public static readonly Key<IImmutableSet<Package>> TransitivePackageDependencies = nameof(TransitivePackageDependencies);

    public static readonly Conf PackageDependenceSupport = Conf
      .Empty
      .InitValue(PackageDependencies, ImmutableHashSet<Package>.Empty)
      .Init(TransitivePackageDependencies, CollectTransitivePackageDependencies);

    public static Conf CSharpProject(string projectId)
      => CSharpProject(projectId, projectId);

    public static Conf CSharpProject(string projectDir, string projectId)
      => Project(projectDir, projectId)
        .AddSources(fileFilter: "*.cs")
        .ExcludeSourceDirs("obj", "bin", TargetDirName)
        .Modify(Input, AddAssemblyReferencesToInput)
        .Init(Compile, DefaultCSharpCompilation)
        .Set(Build, c => Compile[c].Select(CompileOutput.ToInOut))
        .Init(AssemblyName, c => ProjectId[c] + CSharpCompilationOptions[c].OutputKind.ToExtension())
        .Init(AssemblyReferences, c => ImmutableList.Create(typeof(object).Assembly.Location))
        .Init(Compiler, TimedEmittingCompiler.Create)
        .InitValue(CSharpCompilationOptions, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, warningLevel: 1))
        .InitValue(EmbeddedResources, ImmutableList<ResourceDescription>.Empty)
        .Add(PackageDependenceSupport);

    private static IImmutableSet<Package> CollectTransitivePackageDependencies(IConf c)
      => Dependencies[c].Select(dependency => c.TryGet(dependency / TransitivePackageDependencies)
                                               .GetOrElse(ImmutableHashSet<Package>.Empty))
                        .Concat(new[] {PackageDependencies[c]})
                        .SelectMany(set => set)
                        .ToImmutableHashSet();

    private static IObservable<InOut> AddAssemblyReferencesToInput(IConf c, IObservable<InOut> input)
      => input.CombineLatest(Return(AssemblyReferences[c]),
                             (inOut, references) => inOut.Add(references.Select(Assembly.ToAssembly)));

    private static IObservable<CompileOutput> DefaultCSharpCompilation(IConf conf)
      => Input[conf].Select(Compiler[conf])
                    .Do(PrintCompilationResult);

    public static Conf EmbedResource(this Conf conf, string path, string nameInAssembly)
      => conf.Modify(EmbeddedResources,
                     (c, embeddedResources) => {
                       var resourceFile = IsPathRooted(path) ? path : Combine(ProjectDir[c], path);
                       return embeddedResources.Add(ToResourceDescriptor(resourceFile, nameInAssembly));
                     });

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

    private static ResourceDescription ToResourceDescriptor(string resourceFile, string nameInAssembly)
      => new ResourceDescription(nameInAssembly, () => File.OpenRead(resourceFile), true);
  }
}