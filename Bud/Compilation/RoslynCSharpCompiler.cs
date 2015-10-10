using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Compilation {
  public class RoslynCSharpCompiler : ICSharpCompiler {
    public IObservable<CSharpCompilation> Compile(IObservable<FilesUpdate> sourceFiles, IObservable<IEnumerable<MetadataReference>> observedReferences, string assemblyName, CSharpCompilationOptions options) {
      var cSharpCompilation = CSharpCompilation.Create(assemblyName, Enumerable.Empty<SyntaxTree>(), Enumerable.Empty<MetadataReference>(), options);
      var allSyntaxTrees = ImmutableDictionary<string, SyntaxTree>.Empty;

      return FilesDiff.DoDiffing(sourceFiles).CombineLatest(observedReferences, (sources, references) => {
        var addedSources = sources.AddedFiles.Select(ToFileSyntaxTreePair).ToList();
        var changedSources = sources.ChangedFiles.Select(ToFileSyntaxTreePair).ToList();
        var removedSources = sources.RemovedFiles;

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