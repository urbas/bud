using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Bud.Scripting {
  public class RoslynCSharpScriptCompiler : ICSharpScriptCompiler {
    public IImmutableList<Diagnostic> Compile(IEnumerable<string> inputFiles,
                                              IEnumerable<MetadataReference> references,
                                              string outputExe) {
      var syntaxTrees = inputFiles.Select(ParseSyntaxTree)
                                  .ToImmutableList();
      return CSharpCompilation.Create("Bud.Script",
                                      syntaxTrees,
                                      references,
                                      new CSharpCompilationOptions(OutputKind.ConsoleApplication))
                              .Emit(outputExe)
                              .Diagnostics;
    }

    private static SyntaxTree ParseSyntaxTree(string script)
      => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(script), path: script);
  }
}