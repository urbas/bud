using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Bud.Scripting {
  public interface ICSharpScriptCompiler {
    IImmutableList<Diagnostic> Compile(IEnumerable<string> inputFiles,
                                       IEnumerable<MetadataReference> references,
                                       string outputExe);
  }
}