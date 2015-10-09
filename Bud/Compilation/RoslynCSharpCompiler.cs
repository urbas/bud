using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Compilation {
  public class RoslynCSharpCompiler : ICSharpCompiler {
    public IObservable<ICompilationResult> Compile(IObservable<FilesUpdate> sourceFiles,
                                                   string outputDir,
                                                   string assemblyName,
                                                   CSharpCompilationOptions options,
                                                   IEnumerable<MetadataReference> references) {
      var cSharpCompilation = CSharpCompilation.Create(assemblyName, Enumerable.Empty<SyntaxTree>(), references, options);
      var allSyntaxTrees = ImmutableDictionary<string, SyntaxTree>.Empty;

      var assemblyPath = Path.Combine(outputDir, assemblyName);
      Directory.CreateDirectory(outputDir);

      return FilesDiff.DoDiffing(sourceFiles).Select(sources => {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var assemblyOutputFile = File.Create(assemblyPath)) {
          var addedSyntaxTrees = sources.AddedFiles.Select(ToFileSyntaxTreePair).ToList();
          var changedSyntaxTrees = sources.ChangedFiles.Select(ToFileSyntaxTreePair).ToList();

          cSharpCompilation = UpdateCompilation(cSharpCompilation, addedSyntaxTrees, changedSyntaxTrees, sources.RemovedFiles, allSyntaxTrees);
          allSyntaxTrees = UpdateSyntaxTrees(allSyntaxTrees, addedSyntaxTrees, changedSyntaxTrees, sources.RemovedFiles);

          var emitResult = cSharpCompilation.Emit(assemblyOutputFile);

          stopwatch.Stop();
          return new CompilationResult(assemblyPath, emitResult, stopwatch.Elapsed);
        }
      });
    }

    private static KeyValuePair<string, SyntaxTree> ToFileSyntaxTreePair(string s)
      => new KeyValuePair<string, SyntaxTree>(s, SyntaxFactory.ParseSyntaxTree(File.ReadAllText(s)));

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