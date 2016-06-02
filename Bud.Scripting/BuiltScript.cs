namespace Bud.Scripting {
  public class BuiltScript {
    public ResolvedReferences ResolvedReferences { get; }
    public string ScriptExePath { get; }

    public BuiltScript(ResolvedReferences resolvedReferences, string scriptExePath) {
      ResolvedReferences = resolvedReferences;
      ScriptExePath = scriptExePath;
    }
  }
}