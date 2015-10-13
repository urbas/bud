using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Bud.IO.FileTimestamps;

namespace Bud.Compilation {
  public class RoslynCSharpCompiler {
    public IConfigs Configs { get; }
    private ImmutableDictionary<string, SyntaxTree> oldSyntaxTrees = ImmutableDictionary<string, SyntaxTree>.Empty;
    private Diff<string> sources = Diff.Empty<string>();
    private Diff<Dependency> oldDependencies = Diff.Empty<Dependency>();
    private CSharpCompilation cSharpCompilation;

    public RoslynCSharpCompiler(IConfigs configs) {
      Configs = configs;
      cSharpCompilation = CSharpCompilation.Create(CSharp.AssemblyName[Configs], Enumerable.Empty<SyntaxTree>(), Enumerable.Empty<MetadataReference>(), CSharp.CSharpCompilationOptions[Configs]);
    }

    public CSharpCompilation Compile(CompilationInput input) {
      sources = sources.NextDiff(ToTimestampedFiles(input.Sources));
      var newDependenciesDiff = oldDependencies.NextDiff(input.Dependencies);

      var addedSources = sources.Added.Select(ToFileSyntaxTreePair).ToList();
      var changedSources = sources.Changed.Select(ToFileSyntaxTreePair).ToList();
      cSharpCompilation = cSharpCompilation.AddSyntaxTrees(addedSources.Select(pair => pair.Value))
                                           .RemoveSyntaxTrees(sources.Removed.Select(s => oldSyntaxTrees[s]))
                                           .RemoveSyntaxTrees(changedSources.Select(s => oldSyntaxTrees[s.Key]))
                                           .AddSyntaxTrees(changedSources.Select(s => s.Value));
      oldSyntaxTrees = oldSyntaxTrees.RemoveRange(sources.Removed).AddRange(addedSources).SetItems(changedSources);

      cSharpCompilation = UpdateReferences(cSharpCompilation, oldDependencies, newDependenciesDiff);
      oldDependencies = newDependenciesDiff;

      return cSharpCompilation;
    }

    private static CSharpCompilation UpdateReferences(CSharpCompilation cSharpCompilation, Diff<Dependency> previousDependenciesDiff, Diff<Dependency> newDependenciesDiff)
      => cSharpCompilation.RemoveReferences(newDependenciesDiff.Removed.Select(dependency => dependency.MetadataReference))
                          .AddReferences(newDependenciesDiff.Added.Select(dependency => dependency.MetadataReference))
                          .RemoveReferences(newDependenciesDiff.Changed.Select(dependency => GetDependency(previousDependenciesDiff, dependency).MetadataReference))
                          .AddReferences(newDependenciesDiff.Changed.Select(dependency => dependency.MetadataReference));

    private static Dependency GetDependency(Diff<Dependency> dependenciesDiff, Dependency dependency)
      => dependenciesDiff.All.TryGetValue(dependency, out dependency) ? dependency : dependency;

    private static KeyValuePair<string, SyntaxTree> ToFileSyntaxTreePair(string s)
      => new KeyValuePair<string, SyntaxTree>(s, SyntaxFactory.ParseSyntaxTree(File.ReadAllText(s), path: s));
  }
}