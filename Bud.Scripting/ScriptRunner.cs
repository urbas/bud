using System;

namespace Bud.Scripting {
  public class ScriptRunner {
    /// <summary>
    ///   Runs the given script in a new process.
    /// </summary>
    /// <param name="scriptPath">the path to the script to execute.</param>
    /// <param name="args">arguments that will be passed to the script's main method.</param>
    /// <param name="cwd">the working directory in which to run the script.</param>
    public static void Run(string scriptPath, string[] args, string cwd) {
      string executable = null;
      try {
        executable = ScriptBuilder.Build(scriptPath);
        BatchExec.Run(executable, string.Join(" ", args), cwd);
      } catch (Exception e) {
        if (executable == null) {
          Console.Error.WriteLine("An error occurred while building the script. Error message:\n\n    " + e.Message);
          Environment.Exit(1);
        } else {
          throw;
        }
      }
    }
  }
}