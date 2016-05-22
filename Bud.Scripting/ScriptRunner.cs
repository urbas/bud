namespace Bud.Scripting {
  public class ScriptRunner {
    public static void Run(string scriptPath, string args, string cwd) {
      var executable = ScriptBuilder.Build(scriptPath);
      BatchExec.Run(executable, args, cwd);
    }
  }
}