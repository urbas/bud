using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Bud.Dist;
using Bud.Util;
using Newtonsoft.Json;

namespace Bud.Benchmarks {
  public class BenchmarkResults {
    public BenchmarkResults(string vcsRevision,
                            string context,
                            IImmutableList<Measurement> measurements) {
      VcsRevision = vcsRevision;
      Context = context;
      Measurements = measurements;
    }

    public string VcsRevision { get; }
    public string Context { get; }
    public IImmutableList<Measurement> Measurements { get; }

    public string ToJsonFile(string filePath) {
      using (var fileWriter = File.Open(filePath, FileMode.Create, FileAccess.Write)) {
        using (var textFileWriter = new StreamWriter(fileWriter)) {
          WriteJson(textFileWriter);
        }
      }
      return filePath;
    }

    /// <summary>
    /// JSON serializes this object and pushes it to bintray.
    /// </summary>
    /// <param name="repositoryId">the repository id to which to push the benchmark JSON.</param>
    /// <param name="packageId">the name of the BinTray package to upload.</param>
    /// <param name="username">the BinTray username to use when calling into BinTray's API.</param>
    /// <returns>the url from which the uploaded benchmark JSON can be downloaded.</returns>
    public Option<string> PushToBintray(string repositoryId, string packageId, string username)
      => BinTrayDistribution.PushToBintray(
        () => new MemoryStream(Encoding.UTF8.GetBytes(ToJson())),
        repositoryId,
        packageId,
        $"{DateTime.Now.ToString("yyyy.M.d-bHHmmss")}-{VcsRevision.Substring(0, 8)}",
        username,
        "json");

    public void WriteJson(TextWriter writer)
      => JsonSerializer.CreateDefault(new JsonSerializerSettings {Formatting = Formatting.Indented})
                       .Serialize(writer, this);

    public string ToJson() {
      using (var stringWriter = new StringWriter()) {
        WriteJson(stringWriter);
        return stringWriter.ToString();
      }
    }
  }
}