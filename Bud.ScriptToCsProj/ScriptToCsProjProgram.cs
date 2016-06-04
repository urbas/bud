using Bud.Scripting;

namespace Bud.ScriptToCsProj {
  public class ScriptToCsProjProgram {
    public static void Main(string[] args)
      => ScriptCsProj.OutputScriptCsProj(ScriptRunner.LoadBuiltScriptMetadata());
  }
}