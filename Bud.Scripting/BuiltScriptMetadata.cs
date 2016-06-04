namespace Bud.Scripting {
  public class BuiltScriptMetadata {
    public ResolvedScriptReferences ResolvedScriptReferences { get; }
    public string ScriptExePath { get; }

    public BuiltScriptMetadata(ResolvedScriptReferences resolvedScriptReferences, string scriptExePath) {
      ResolvedScriptReferences = resolvedScriptReferences;
      ScriptExePath = scriptExePath;
    }
  }
}