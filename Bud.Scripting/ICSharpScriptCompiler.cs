using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.References;
using Microsoft.CodeAnalysis;

namespace Bud.Scripting {
  public interface ICSharpScriptCompiler {
    IImmutableList<Diagnostic> Compile(IEnumerable<string> inputFiles,
                                       ResolvedReferences references,
                                       string outputExe);
  }
}