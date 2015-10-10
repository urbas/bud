using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Bud.IO.FileTimestamps;

namespace Bud.Compilation {
  public class RoslynCSharpCompiler : ICSharpCompiler {
    public IObservable<CSharpCompilation> Compile(IObservable<IEnumerable<string>> sourceFiles, IObservable<IEnumerable<MetadataReference>> observedReferences, string assemblyName, CSharpCompilationOptions options) {
      var cSharpCompilation = CSharpCompilation.Create(assemblyName, Enumerable.Empty<SyntaxTree>(), Enumerable.Empty<MetadataReference>(), options);
      var allSyntaxTrees = ImmutableDictionary<string, SyntaxTree>.Empty;
      var sourceDiff = Diff.Empty<string>();

      return sourceFiles.CombineLatest(observedReferences, (sources, references) => {
        sourceDiff = sourceDiff.NextDiff(ToTimestampedFiles(sources));
        var addedSources = sourceDiff.Added.Select(ToFileSyntaxTreePair).ToList();
        var changedSources = sourceDiff.Changed.Select(ToFileSyntaxTreePair).ToList();
        var removedSources = sourceDiff.Removed;

        sourceDiff.ToPrettyString();

        cSharpCompilation = UpdateCompilation(cSharpCompilation, addedSources, changedSources, removedSources, allSyntaxTrees);
        allSyntaxTrees = UpdateSyntaxTrees(allSyntaxTrees, addedSources, changedSources, removedSources);
        cSharpCompilation = cSharpCompilation.RemoveAllReferences().AddReferences(references);
        return cSharpCompilation;
      });
    }

    private static KeyValuePair<string, SyntaxTree> ToFileSyntaxTreePair(string s)
      => new KeyValuePair<string, SyntaxTree>(s, SyntaxFactory.ParseSyntaxTree(File.ReadAllText(s), path: s));

    private static ImmutableDictionary<string, SyntaxTree> UpdateSyntaxTrees(ImmutableDictionary<string, SyntaxTree> allSyntaxTrees, IEnumerable<KeyValuePair<string, SyntaxTree>> addedSources, IEnumerable<KeyValuePair<string, SyntaxTree>> changedSources, ImmutableHashSet<string> removedFiles)
      => allSyntaxTrees.RemoveRange(removedFiles)
                       .AddRange(addedSources)
                       .SetItems(changedSources);

    private static CSharpCompilation UpdateCompilation(CSharpCompilation cSharpCompilation, IEnumerable<KeyValuePair<string, SyntaxTree>> addedSources, IEnumerable<KeyValuePair<string, SyntaxTree>> changedSources, ImmutableHashSet<string> removedSources, IDictionary<string, SyntaxTree> filesToSyntaxTrees) {
      var updatedCompilation = cSharpCompilation.AddSyntaxTrees(addedSources.Select(pair => pair.Value))
                                                .RemoveSyntaxTrees(removedSources.Select(s => filesToSyntaxTrees[s]));
      foreach (var source in changedSources) {
        updatedCompilation = updatedCompilation.ReplaceSyntaxTree(filesToSyntaxTrees[source.Key], source.Value);
      }
      return updatedCompilation;
    }
  }
}