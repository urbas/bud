using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Bud.Util;
using Bud.V1;
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
    ///   JSON serializes this object and pushes it to bintray.
    /// </summary>
    /// <param name="repositoryId">the repository id to which to push the benchmark JSON.</param>
    /// <param name="packageId">the name of the BinTray package to upload.</param>
    /// <param name="username">the BinTray username to use when calling into BinTray's API.</param>
    /// <returns>the url from which the uploaded benchmark JSON can be downloaded.</returns>
    public Option<string> PushToBintray(string repositoryId, string packageId, string username)
      => BinTrayPublishing.PushToBintray(
        () => new MemoryStream(Encoding.UTF8.GetBytes((string) ToJson())),
        repositoryId,
        packageId,
        $"{DateTime.Now.ToString("yyyy.M.d-bHHmmss")}-{VcsRevision.Substring(0, 8)}",
        username,
        "json");

    public void WriteJson(TextWriter writer)
      => JsonSerializer.CreateDefault(new JsonSerializerSettings {Formatting = Formatting.Indented})
                       .Serialize(writer, this);

    public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

    public static BenchmarkResults FromJson(string json)
      => JsonConvert.DeserializeObject<BenchmarkResults>(json);

    protected bool Equals(BenchmarkResults other)
      => string.Equals(Context, other.Context) &&
         Measurements.SequenceEqual(other.Measurements) &&
         string.Equals(VcsRevision, other.VcsRevision);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      return obj.GetType() == this.GetType() && Equals((BenchmarkResults) obj);
    }

    public override int GetHashCode() {
      unchecked {
        var hashCode = Context.GetHashCode();
        hashCode = (hashCode*397) ^ Measurements.GetHashCode();
        hashCode = (hashCode*397) ^ VcsRevision.GetHashCode();
        return hashCode;
      }
    }

    public static bool operator ==(BenchmarkResults left, BenchmarkResults right) {
      return Equals(left, right);
    }

    public static bool operator !=(BenchmarkResults left, BenchmarkResults right) {
      return !Equals(left, right);
    }

    public override string ToString() => ToJson();
  }
}