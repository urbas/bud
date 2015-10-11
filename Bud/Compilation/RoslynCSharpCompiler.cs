using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bud.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Bud.IO.FileTimestamps;

namespace Bud.Compilation {
  public class RoslynCSharpCompiler : ICSharpCompiler {
    public IObservable<CSharpCompilation> Compile(IObservable<CSharpCompilationInput> inputPipe, IConfigs config) {
      var cSharpCompilation = CSharpCompilation.Create(CSharp.AssemblyName[config], Enumerable.Empty<SyntaxTree>(), Enumerable.Empty<MetadataReference>(), CSharp.CSharpCompilationOptions[config]);
      var allSyntaxTrees = ImmutableDictionary<string, SyntaxTree>.Empty;
      var sourceDiff = Diff.Empty<string>();
      var previousDependenciesDiff = Diff.Empty<Dependency>();

      return inputPipe.Select(input => {
        sourceDiff = sourceDiff.NextDiff(ToTimestampedFiles(input.Sources));
        var dependenciesDiff = previousDependenciesDiff.NextDiff(input.Dependencies as IList<Timestamped<Dependency>> ?? input.Dependencies.ToList());

        Console.WriteLine($"=================");
        Console.Write(sourceDiff.ToPrettyString());
        Console.Write(dependenciesDiff.ToPrettyString());

        var addedSources = sourceDiff.Added.Select(ToFileSyntaxTreePair).ToList();
        var changedSources = sourceDiff.Changed.Select(ToFileSyntaxTreePair).ToList();
        cSharpCompilation = UpdateCompilation(cSharpCompilation, addedSources, changedSources, sourceDiff.Removed, allSyntaxTrees);
        allSyntaxTrees = UpdateSyntaxTrees(allSyntaxTrees, addedSources, changedSources, sourceDiff.Removed);

        cSharpCompilation = UpdateReferences(dependenciesDiff, cSharpCompilation, previousDependenciesDiff);
        previousDependenciesDiff = dependenciesDiff;

        return cSharpCompilation;
      });
    }

    private static CSharpCompilation UpdateReferences(Diff<Dependency> newDependenciesDiff, CSharpCompilation cSharpCompilation, Diff<Dependency> previousDependenciesDiff)
      => cSharpCompilation.RemoveReferences(newDependenciesDiff.Removed.Select(dependency => dependency.MetadataReference))
                          .AddReferences(newDependenciesDiff.Added.Select(dependency => dependency.MetadataReference))
                          .RemoveReferences(newDependenciesDiff.Changed.Select(dependency => GetDependency(previousDependenciesDiff, dependency).MetadataReference))
                          .AddReferences(newDependenciesDiff.Changed.Select(dependency => dependency.MetadataReference));

    private static Dependency GetDependency(Diff<Dependency> dependenciesDiff, Dependency dependency)
      => dependenciesDiff.All.TryGetValue(dependency, out dependency) ? dependency : dependency;

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