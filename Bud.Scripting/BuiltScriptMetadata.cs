using Bud.References;

namespace Bud.Scripting {
  public class BuiltScriptMetadata {
    public ResolvedReferences ResolvedScriptReferences { get; }
    public string ScriptExePath { get; }

    public BuiltScriptMetadata(ResolvedReferences resolvedScriptReferences, string scriptExePath) {
      ResolvedScriptReferences = resolvedScriptReferences;
      ScriptExePath = scriptExePath;
    }
  }
}