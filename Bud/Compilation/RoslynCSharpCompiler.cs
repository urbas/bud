using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Compilation {
  public class RoslynCSharpCompiler {
    public IConf Conf { get; }
    private ImmutableDictionary<Timestamped<string>, SyntaxTree> syntaxTrees = ImmutableDictionary<Timestamped<string>, SyntaxTree>.Empty;
    private Diff<Timestamped<string>> sources = Diff.Empty<Timestamped<string>>();
    private Diff<Timestamped<Dependency>> oldDependencies = Diff.Empty<Timestamped<Dependency>>();
    private CSharpCompilation cSharpCompilation;

    public RoslynCSharpCompiler(IConf conf) {
      Conf = conf;
      cSharpCompilation = CSharpCompilation.Create(CSharp.AssemblyName[Conf],
                                                   Enumerable.Empty<SyntaxTree>(),
                                                   Enumerable.Empty<MetadataReference>(),
                                                   CSharp.CSharpCompilationOptions[Conf]);
    }

    public CSharpCompilation Compile(CompilationInput input) {
      sources = sources.NextDiff(input.Sources);
      var newDependencies = oldDependencies.NextDiff(input.Assemblies);

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

    private static Timestamped<Dependency> GetDependency(Diff<Timestamped<Dependency>> dependenciesDiff, Timestamped<Dependency> dependency)
      => dependenciesDiff.All.TryGetValue(dependency, out dependency) ? dependency : dependency;

    private static KeyValuePair<Timestamped<string>, SyntaxTree> ToFileSyntaxTreePair(Timestamped<string> s)
      => new KeyValuePair<Timestamped<string>, SyntaxTree>(s, SyntaxFactory.ParseSyntaxTree(File.ReadAllText(s.Value), path: s.Value));

    private static MetadataReference ToReference(Timestamped<Dependency> dependency)
      => dependency.Value.MetadataReference;

    private MetadataReference FindOldReference(Timestamped<Dependency> dependency)
      => GetDependency(oldDependencies, dependency).Value.MetadataReference;
  }
}