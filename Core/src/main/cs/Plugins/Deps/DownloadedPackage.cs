using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Plugins.Deps {
  public class DownloadedPackage {
    public readonly List<string> AssemblyPaths;
    public readonly SemanticVersion Version;

    private DownloadedPackage(SemanticVersion version, List<string> assemblyPaths) {
      AssemblyPaths = assemblyPaths;
      Version = version;
    }

    public DownloadedPackage(IPackage package) {
      Version = package.Version;
      AssemblyPaths = package.AssemblyReferences.Select(assemblyReference => assemblyReference.Path).ToList();
    }

    public void ToJson(JsonTextWriter jsonWriter) {
      jsonWriter.WriteStartArray();
      jsonWriter.WriteValue(Version.ToString());
      WriteAssemblyPathsToJson(jsonWriter);
      jsonWriter.WriteEndArray();
    }

    private void WriteAssemblyPathsToJson(JsonTextWriter jsonWriter) {
      jsonWriter.WriteStartArray();
      foreach (var assemblyPath in AssemblyPaths) {
        jsonWriter.WriteValue(assemblyPath);
      }
      jsonWriter.WriteEndArray();
    }

    public static DownloadedPackage FromJson(JsonReader jsonReader) {
      jsonReader.Read();
      var version = SemanticVersion.Parse((string) jsonReader.Value);
      jsonReader.Read();
      var assemblyPaths = new List<string>();
      while (jsonReader.Read() && jsonReader.TokenType != JsonToken.EndArray) {
        assemblyPaths.Add((string) jsonReader.Value);
      }
      return new DownloadedPackage(version, assemblyPaths);
    }
  }
}