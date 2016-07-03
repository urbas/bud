using System.IO;
using Bud.References;
using Newtonsoft.Json;

namespace Bud.Scripting {
  public class BuiltScriptMetadata {
    public ResolvedReferences ResolvedScriptReferences { get; }

    public BuiltScriptMetadata(ResolvedReferences resolvedScriptReferences) {
      ResolvedScriptReferences = resolvedScriptReferences;
    }

    /// <summary>
    ///   Loads the metadata of the script in the current working directory.
    ///   This method will build the script first (as the metadata is available only for
    ///   built scripts).
    /// </summary>
    /// <param name="scriptExePath">
    ///   the path to the script executable. The "built script metadata" JSON file
    ///   is placed next to the script executable.
    /// </param>
    /// <returns>the metadata of the script.</returns>
    public static BuiltScriptMetadata Load(string scriptExePath) {
      var scriptMetadataPath = ScriptBuilder.ScriptMetadataPath(scriptExePath);
      return JsonConvert.DeserializeObject<BuiltScriptMetadata>(File.ReadAllText(scriptMetadataPath));
    }

    /// <summary>
    ///   Stores a json file next to the given script executable.
    /// </summary>
    /// <param name="scriptPath">
    ///   the path of the script that will be built and whose metadata we seek.
    /// </param>
    /// <param name="references">
    ///   a list of non-framework assemblies and framework assemblies referenced by the script.
    /// </param>
    /// <returns></returns>
    public static BuiltScriptMetadata Save(string scriptPath, ResolvedReferences references) {
      var builtScriptMetadata = new BuiltScriptMetadata(references);
      var builtScriptMetadataJson = JsonConvert.SerializeObject(builtScriptMetadata, Formatting.Indented);
      File.WriteAllText(ScriptBuilder.ScriptMetadataPath(scriptPath), builtScriptMetadataJson);
      return builtScriptMetadata;
    }
  }
}