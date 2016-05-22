using System.Diagnostics;

namespace Bud.Scripting {
  public class ScriptRunner {
    public static void Run(string scriptPath, string cwd, string[] args) {
      // Build the script or load the executable from the cache
      var executable = ScriptBuilder.Build(scriptPath);
      // run the executable in a fresh process.
      new Process();
    }
  }

  public class ScriptBuilder {
    /// <param name="scriptPath">
    ///   the path of the C# script file to build.
    /// </param>
    /// <returns>
    ///   the path to the produced executable.
    ///   The executable can be run as is.
    /// </returns>
    public static string Build(string scriptPath) {
      throw new System.NotImplementedException();
    }
  }
}