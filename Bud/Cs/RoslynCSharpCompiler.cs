using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Bud.IO.FileUtils;

namespace Bud.Cs {
  public class RoslynCSharpCompiler : ICompiler {
    private Diff<Timestamped<string>> sources
      = Diff.Empty<Timestamped<string>>();

    private ImmutableDictionary<Timestamped<string>, SyntaxTree> syntaxTreesCache
      = ImmutableDictionary<Timestamped<string>, SyntaxTree>.Empty;

    private Diff<Timestamped<string>> references
      = Diff.Empty<Timestamped<string>>();

    private ImmutableDictionary<Timestamped<string>, MetadataReference> referencesCache
      = ImmutableDictionary<Timestamped<string>, MetadataReference>.Empty;

    private CSharpCompilation cSharpCompilation;

    public RoslynCSharpCompiler(string assemblyName, CSharpCompilationOptions compilationOptions) {
      cSharpCompilation = CSharpCompilation
        .Create(assemblyName,
                Enumerable.Empty<SyntaxTree>(),
                Enumerable.Empty<MetadataReference>(),
                compilationOptions);
    }

    public CSharpCompilation Compile(IEnumerable<string> sources,
                                     IEnumerable<string> assemblyReferences)
      => Compile(ToTimestampedFiles(sources),
                 ToTimestampedFiles(assemblyReferences));

    public CSharpCompilation Compile(IEnumerable<Timestamped<string>> inputSources,
                                     IEnumerable<Timestamped<string>> inputAssemblies) {
      UpdateSources(inputSources);
      UpdateReferences(inputAssemblies);
      return cSharpCompilation;
    }

    private void UpdateReferences(IEnumerable<Timestamped<string>> newAssemblies) {
      references = references.DiffByTimestamp(newAssemblies);
      var oldReferencesCache = referencesCache;
      referencesCache = Diff.UpdateCache(referencesCache, references, LoadAssemblyFromFile);
      cSharpCompilation = cSharpCompilation.AddReferences(references.Added.Select(r => referencesCache[r]))
                                           .RemoveReferences(references.Removed.Select(r => oldReferencesCache[r]))
                                           .RemoveReferences(references.Changed.Select(r => oldReferencesCache[r]))
                                           .AddReferences(references.Changed.Select(r => referencesCache[r]));
    }

    private void UpdateSources(IEnumerable<Timestamped<string>> newSources) {
      sources = sources.DiffByTimestamp(newSources);
      var oldSyntaxTreesCache = syntaxTreesCache;
      syntaxTreesCache = Diff.UpdateCache(syntaxTreesCache, sources, ParseSyntaxTree);
      cSharpCompilation = cSharpCompilation.AddSyntaxTrees(sources.Added.Select(s => syntaxTreesCache[s]))
                                           .RemoveSyntaxTrees(sources.Removed.Select(s => oldSyntaxTreesCache[s]))
                                           .RemoveSyntaxTrees(sources.Changed.Select(s => oldSyntaxTreesCache[s]))
                                           .AddSyntaxTrees(sources.Changed.Select(s => syntaxTreesCache[s]));
    }

    private static SyntaxTree ParseSyntaxTree(Timestamped<string> s)
      => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(s.Value), path: s.Value);

    private static MetadataReference LoadAssemblyFromFile(Timestamped<string> path)
      => MetadataReference.CreateFromFile(path.Value);
  }
}