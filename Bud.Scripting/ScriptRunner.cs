using System.IO;
using static Newtonsoft.Json.JsonConvert;

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
      => BatchExec.Run(Build(scriptPath), string.Join(" ", args), workingDir);

    /// <summary>
    ///   Loads the metadata of the script in the current working directory.
    ///   This method will build the script first (as the metadata is available only for
    ///   built scripts).
    /// </summary>
    /// <param name="scriptPath">
    ///   the path of the script that will be built and whose metadata we seek.
    /// </param>
    /// <returns>the metadata.</returns>
    public static BuiltScriptMetadata LoadBuiltScriptMetadata(Option<string> scriptPath = default(Option<string>)) {
      var scriptMetadataPath = ScriptBuilder.ScriptMetadataPath(Build(scriptPath));
      return DeserializeObject<BuiltScriptMetadata>(File.ReadAllText(scriptMetadataPath));
    }

    /// <summary>
    ///   The default way of building <c>Build.cs</c> scripts.
    /// </summary>
    /// <returns>the path to the built executable. This executable can be run as is.</returns>
    private static string Build(Option<string> scriptPath = default(Option<string>))
      => ScriptBuilder.Build(CoalesceScriptPath(scriptPath), new BudReferenceResolver());

    private static string CoalesceScriptPath(Option<string> scriptPath)
      => scriptPath.HasValue ? scriptPath.Value : DefaultScriptPath;
  }
}