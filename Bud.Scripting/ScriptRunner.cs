using System.IO;
using System.Reflection;

namespace Bud.Scripting {
  public class ScriptRunner {
    /// <summary>
    ///   Runs the given script in a new process.
    /// </summary>
    /// <param name="scriptPath">the path to the script to execute.</param>
    /// <param name="args">arguments that will be passed to the script's main method.</param>
    /// <param name="cwd">the working directory in which to run the script.</param>
    public static void Run(string scriptPath, string[] args, string cwd) {
      var executable = ScriptBuilder.Build(scriptPath);
      BatchExec.Run(executable, string.Join(" ", args), cwd);
    }
  }
}