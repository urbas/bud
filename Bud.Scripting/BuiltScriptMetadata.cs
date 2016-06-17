using Bud.References;

namespace Bud.Scripting {
  public class BuiltScriptMetadata {
    public ResolvedReferences ResolvedScriptReferences { get; }

    public BuiltScriptMetadata(ResolvedReferences resolvedScriptReferences) {
      ResolvedScriptReferences = resolvedScriptReferences;
    }
  }
}