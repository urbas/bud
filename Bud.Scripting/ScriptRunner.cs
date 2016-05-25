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

    /// <summary>
    ///   Runs the given script in the current process.
    /// </summary>
    /// <param name="scriptPath">the path to the script to execute.</param>
    /// <param name="args">arguments that will be passed to the script's main method.</param>
    /// <param name="cwd">the working directory in which to run the script.</param>
    /// <remarks>
    ///   This method loads the script's assembly into the current app domain and executes its
    ///   entry point (the main method). Note that the assembly will therefore never be unloaded.
    /// </remarks>
    public static void RunInProcess(string scriptPath, string[] args, string cwd) {
      var executable = ScriptBuilder.Build(scriptPath);
      var assembly = Assembly.LoadFile(executable);
      var oldCwd = Directory.GetCurrentDirectory();
      try {
        Directory.SetCurrentDirectory(cwd);
        assembly.EntryPoint.Invoke(null, new object[] {args});
      } finally {
        Directory.SetCurrentDirectory(oldCwd);
      }
    }
  }
}