using System.IO;

namespace Bud.Scripting {
  public class ScriptRunner {
    /// <summary>
    ///   The default script name is <c>Build.cs</c> and is looked up in the current working directory.
    /// </summary>
    public static string DefaultScriptPath
      => Path.Combine(Directory.GetCurrentDirectory(), "Build.cs");

    /// <summary>
    ///   Runs the given script in a new process in the current working directory.
    /// </summary>
    /// <param name="args">arguments that will be passed to the script's main method.</param>
    /// <param name="scriptPath">
    ///   the path to the script that we want to run.
    ///   If not given, <see cref="DefaultScriptPath" /> is used.
    /// </param>
    /// <param name="workingDir">
    ///   the working directory in which to run the script's process.
    ///   If not given, uses the current working directory.
    /// </param>
    public static int Run(string[] args,
                          Option<string> scriptPath = default(Option<string>),
                          Option<string> workingDir = default(Option<string>))
      => BatchExec.Run(ScriptBuilder.Build(scriptPath), string.Join(" ", args), workingDir);
  }
}