using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Bud.IO.FileUtils;

namespace Bud.Cs {
  public class RoslynCSharpCompiler {
    private readonly ValueUpdater<Timestamped<string>, SyntaxTree> syntaxTreesUpdater;
    private readonly ValueUpdater<Timestamped<string>, MetadataReference> referencesUpdater;
    private readonly IncrementalCompilation compilation;

    public RoslynCSharpCompiler(string assemblyName, CSharpCompilationOptions compilationOptions) {
      var cSharpCompilation = CSharpCompilation
        .Create(assemblyName,
                Enumerable.Empty<SyntaxTree>(),
                Enumerable.Empty<MetadataReference>(),
                compilationOptions);
      compilation = new IncrementalCompilation(cSharpCompilation);
      syntaxTreesUpdater = new ValueUpdater<Timestamped<string>, SyntaxTree>(compilation, ParseSyntaxTree);
      referencesUpdater = new ValueUpdater<Timestamped<string>, MetadataReference>(compilation, LoadAssemblyFromFile);
    }

    public CSharpCompilation Compile(IEnumerable<string> sources,
                                     IEnumerable<string> assemblyReferences)
      => Compile(ToTimestampedFiles(sources),
                 ToTimestampedFiles(assemblyReferences));

    public CSharpCompilation Compile(IEnumerable<Timestamped<string>> inputSources,
                                     IEnumerable<Timestamped<string>> inputAssemblies) {
      syntaxTreesUpdater.UpdateWith(inputSources);
      referencesUpdater.UpdateWith(inputAssemblies);
      return compilation.State;
    }

    private static SyntaxTree ParseSyntaxTree(Timestamped<string> s)
      => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(s), path: s);

    private static MetadataReference LoadAssemblyFromFile(Timestamped<string> path)
      => MetadataReference.CreateFromFile(path);

    private class IncrementalCompilation
      : IValueStore<SyntaxTree>, IValueStore<MetadataReference> {
      public CSharpCompilation State { get; private set; }

      public IncrementalCompilation(CSharpCompilation state) {
        State = state;
      }

      public void Add(IEnumerable<SyntaxTree> newValues) => State = State.AddSyntaxTrees(newValues);
      public void Remove(IEnumerable<SyntaxTree> oldValues) => State = State.RemoveSyntaxTrees(oldValues);
      public void Add(IEnumerable<MetadataReference> newValues) => State = State.AddReferences(newValues);
      public void Remove(IEnumerable<MetadataReference> oldValues) => State = State.AddReferences(oldValues);
    }
  }
}