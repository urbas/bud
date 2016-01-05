using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Bud.IO.FileUtils;

namespace Bud.Cs {
  public class RoslynCSharpCompiler : ICompiler {
    private readonly ValueUpdater<Timestamped<string>, SyntaxTree> syntaxTreesUpdater;
    private readonly ValueUpdater<Timestamped<string>, MetadataReference> referencesUpdater;
    private IncrementalRoslynCSharpCompiler compiler;

    public RoslynCSharpCompiler(string assemblyName, CSharpCompilationOptions compilationOptions) {
      var cSharpCompilation = CSharpCompilation
        .Create(assemblyName,
                Enumerable.Empty<SyntaxTree>(),
                Enumerable.Empty<MetadataReference>(),
                compilationOptions);
      compiler = new IncrementalRoslynCSharpCompiler(cSharpCompilation);
      syntaxTreesUpdater = new ValueUpdater<Timestamped<string>, SyntaxTree>(compiler, ParseSyntaxTree);
      referencesUpdater = new ValueUpdater<Timestamped<string>, MetadataReference>(compiler, LoadAssemblyFromFile);
    }

    public CSharpCompilation Compile(IEnumerable<string> sources,
                                     IEnumerable<string> assemblyReferences)
      => Compile(ToTimestampedFiles(sources),
                 ToTimestampedFiles(assemblyReferences));

    public CSharpCompilation Compile(IEnumerable<Timestamped<string>> inputSources,
                                     IEnumerable<Timestamped<string>> inputAssemblies) {
      syntaxTreesUpdater.UpdateWith(inputSources);
      referencesUpdater.UpdateWith(inputAssemblies);
      return compiler.Compilation;
    }

    private static SyntaxTree ParseSyntaxTree(Timestamped<string> s)
      => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(s), path: s);

    private static MetadataReference LoadAssemblyFromFile(Timestamped<string> path)
      => MetadataReference.CreateFromFile(path);

    private class IncrementalRoslynCSharpCompiler
      : IValueStore<SyntaxTree>, IValueStore<MetadataReference> {
      public CSharpCompilation Compilation { get; private set; }

      public IncrementalRoslynCSharpCompiler(CSharpCompilation compilation) {
        Compilation = compilation;
      }

      public void Add(IEnumerable<SyntaxTree> newValues) => Compilation = Compilation.AddSyntaxTrees(newValues);
      public void Remove(IEnumerable<SyntaxTree> oldValues) => Compilation = Compilation.RemoveSyntaxTrees(oldValues);
      public void Add(IEnumerable<MetadataReference> newValues) => Compilation = Compilation.AddReferences(newValues);
      public void Remove(IEnumerable<MetadataReference> oldValues) => Compilation = Compilation.AddReferences(oldValues);
    }
  }
}