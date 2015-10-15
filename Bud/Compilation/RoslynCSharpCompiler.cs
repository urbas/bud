using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Bud.IO.FileTimestamps;

namespace Bud.Compilation {
  public class RoslynCSharpCompiler {
    public IConf Conf { get; }
    private ImmutableDictionary<string, SyntaxTree> syntaxTrees = ImmutableDictionary<string, SyntaxTree>.Empty;
    private Diff<string> sources = Diff.Empty<string>();
    private Diff<Dependency> oldDependencies = Diff.Empty<Dependency>();
    private CSharpCompilation cSharpCompilation;

    public RoslynCSharpCompiler(IConf conf) {
      Conf = conf;
      cSharpCompilation = CSharpCompilation.Create(CSharp.AssemblyName[Conf],
                                                   Enumerable.Empty<SyntaxTree>(),
                                                   Enumerable.Empty<MetadataReference>(),
                                                   CSharp.CSharpCompilationOptions[Conf]);
    }

    public CSharpCompilation Compile(CompilationInput input) {
      sources = sources.NextDiff(ToTimestampedFiles(input.Sources));
      var newDependencies = oldDependencies.NextDiff(input.Dependencies);

      var addedSources = sources.Added.Select(ToFileSyntaxTreePair).ToList();
      var changedSources = sources.Changed.Select(ToFileSyntaxTreePair).ToList();
      cSharpCompilation = cSharpCompilation.AddSyntaxTrees(addedSources.Select(pair => pair.Value))
                                           .RemoveSyntaxTrees(sources.Removed.Select(s => syntaxTrees[s]))
                                           .RemoveSyntaxTrees(changedSources.Select(s => syntaxTrees[s.Key]))
                                           .AddSyntaxTrees(changedSources.Select(s => s.Value));

      cSharpCompilation = cSharpCompilation.RemoveReferences(newDependencies.Removed.Select(ToReference))
                                           .AddReferences(newDependencies.Added.Select(ToReference))
                                           .RemoveReferences(newDependencies.Changed.Select(FindOldReference))
                                           .AddReferences(newDependencies.Changed.Select(ToReference));

      syntaxTrees = syntaxTrees.RemoveRange(sources.Removed).AddRange(addedSources).SetItems(changedSources);
      oldDependencies = newDependencies;

      return cSharpCompilation;
    }

    private static Dependency GetDependency(Diff<Dependency> dependenciesDiff, Dependency dependency)
      => dependenciesDiff.All.TryGetValue(dependency, out dependency) ? dependency : dependency;

    private static KeyValuePair<string, SyntaxTree> ToFileSyntaxTreePair(string s)
      => new KeyValuePair<string, SyntaxTree>(s, SyntaxFactory.ParseSyntaxTree(File.ReadAllText(s), path: s));

    private static MetadataReference ToReference(Dependency dependency)
      => dependency.MetadataReference;

    private MetadataReference FindOldReference(Dependency dependency)
      => GetDependency(oldDependencies, dependency).MetadataReference;
  }
}