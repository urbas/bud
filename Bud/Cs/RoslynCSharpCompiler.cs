using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Cs {
  public class RoslynCSharpCompiler {
    private ImmutableDictionary<Timestamped<string>, SyntaxTree> syntaxTrees = ImmutableDictionary<Timestamped<string>, SyntaxTree>.Empty;
    private Diff<Timestamped<string>> sources = Diff.Empty<Timestamped<string>>();
    private Diff<Timestamped<string>> oldDependencies = Diff.Empty<Timestamped<string>>();
    private ImmutableDictionary<Timestamped<string>, MetadataReference> referencesCache = ImmutableDictionary<Timestamped<string>, MetadataReference>.Empty;
    private CSharpCompilation cSharpCompilation;

    public RoslynCSharpCompiler(string assemblyName, CSharpCompilationOptions cSharpCompilationOptions) {
      cSharpCompilation = CSharpCompilation.Create(assemblyName,
                                                   Enumerable.Empty<SyntaxTree>(),
                                                   Enumerable.Empty<MetadataReference>(),
                                                   cSharpCompilationOptions);
    }

    public CSharpCompilation Compile(CompileInput input) {
      sources = sources.NextDiff(input.Sources);
      oldDependencies = oldDependencies.NextDiff(input.Assemblies);
      var oldReferencesCache = referencesCache;
      referencesCache = Diff.UpdateCache(referencesCache, oldDependencies, path => MetadataReference.CreateFromFile(path.Value));
      var oldSyntaxTrees = syntaxTrees;
      syntaxTrees = Diff.UpdateCache(syntaxTrees, sources, s => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(s.Value), path: s.Value));

      cSharpCompilation = cSharpCompilation.AddSyntaxTrees(sources.Added.Select(s => syntaxTrees[s]))
                                           .RemoveSyntaxTrees(sources.Removed.Select(s => oldSyntaxTrees[s]))
                                           .RemoveSyntaxTrees(sources.Changed.Select(s => oldSyntaxTrees[s]))
                                           .AddSyntaxTrees(sources.Changed.Select(s => syntaxTrees[s]));

      cSharpCompilation = cSharpCompilation.AddReferences(oldDependencies.Added.Select(timestamped => referencesCache[timestamped]))
                                           .RemoveReferences(oldDependencies.Removed.Select(timestamped => oldReferencesCache[timestamped]))
                                           .RemoveReferences(oldDependencies.Changed.Select(timestamped => oldReferencesCache[timestamped]))
                                           .AddReferences(oldDependencies.Changed.Select(timestamped => referencesCache[timestamped]));
      return cSharpCompilation;
    }

    public static Func<CompileInput, CSharpCompilation> Create(string assemblyName, CSharpCompilationOptions cSharpCompilationOptions)
      => new RoslynCSharpCompiler(assemblyName, cSharpCompilationOptions).Compile;
  }
}