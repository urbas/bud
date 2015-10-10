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
        var addedSyntaxTrees = sources.AddedFiles.Select(ToFileSyntaxTreePair).ToList();
        var changedSyntaxTrees = sources.ChangedFiles.Select(ToFileSyntaxTreePair).ToList();

        cSharpCompilation = UpdateCompilation(cSharpCompilation, addedSyntaxTrees, changedSyntaxTrees, sources.RemovedFiles, allSyntaxTrees);
        allSyntaxTrees = UpdateSyntaxTrees(allSyntaxTrees, addedSyntaxTrees, changedSyntaxTrees, sources.RemovedFiles);
        cSharpCompilation = cSharpCompilation.RemoveAllReferences().AddReferences(references);
        return cSharpCompilation;
      });
    }

    private static KeyValuePair<string, SyntaxTree> ToFileSyntaxTreePair(string s)
      => new KeyValuePair<string, SyntaxTree>(s, SyntaxFactory.ParseSyntaxTree(File.ReadAllText(s), path: s));

    private static ImmutableDictionary<string, SyntaxTree> UpdateSyntaxTrees(ImmutableDictionary<string, SyntaxTree> allSyntaxTrees, List<KeyValuePair<string, SyntaxTree>> addedSyntaxTrees, List<KeyValuePair<string, SyntaxTree>> changedSyntaxTrees, ImmutableHashSet<string> removedFiles)
      => allSyntaxTrees.RemoveRange(removedFiles)
                       .AddRange(addedSyntaxTrees)
                       .SetItems(changedSyntaxTrees);

    private static CSharpCompilation UpdateCompilation(CSharpCompilation cSharpCompilation, List<KeyValuePair<string, SyntaxTree>> addedSyntaxTrees, List<KeyValuePair<string, SyntaxTree>> changedSyntaxTrees, ImmutableHashSet<string> removedFiles, ImmutableDictionary<string, SyntaxTree> filesToSyntaxTrees)
      => changedSyntaxTrees.Aggregate(cSharpCompilation.AddSyntaxTrees(addedSyntaxTrees.Select(pair => pair.Value))
                                                       .RemoveSyntaxTrees(removedFiles.Select(s => filesToSyntaxTrees[s])),
                                      (compilation, pair) => compilation.ReplaceSyntaxTree(filesToSyntaxTrees[pair.Key], pair.Value));
  }
}