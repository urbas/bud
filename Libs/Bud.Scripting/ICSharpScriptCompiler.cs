using System.Collections.Immutable;
using Bud.References;
using Microsoft.CodeAnalysis;

namespace Bud.Scripting {
  public interface ICSharpScriptCompiler {
    ImmutableArray<Diagnostic> Compile(ImmutableArray<string> inputFiles,
                                       ResolvedReferences references,
                                       string outputExe);
  }
}