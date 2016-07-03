using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace Bud.Building {
  public class Digest {
    public DateTime Timestamp { get; }
    public string Hash { get; }

    public Digest(DateTime timestamp, string hash) {
      Hash = hash;
      Timestamp = timestamp;
    }

    public static void CreateDigestsJsonFile(IEnumerable<string> inputFiles, string outputFile)
      => File.WriteAllText(outputFile, DigestsJson(inputFiles));

    public static Option<IReadOnlyDictionary<string, Digest>> LoadDigestsJsonFile(string digestsJsonFile)
      => new ReadOnlyDictionary<string, Digest>(
        JsonConvert.DeserializeObject<Dictionary<string, Digest>>(
          File.ReadAllText(digestsJsonFile)));

    private static string DigestsJson(IEnumerable<string> inputFiles)
      => JsonConvert.SerializeObject(CreateDigests(inputFiles));

    private static string GetMd5Hash(string file) {
      using (var md5 = MD5.Create()) {
        using (var stream = File.OpenRead(file)) {
          var computeHash = md5.ComputeHash(stream);
          return BitConverter.ToString(computeHash).Replace("-", "").ToLower();
        }
      }
    }

    private static IReadOnlyDictionary<string, Digest> CreateDigests(IEnumerable<string> inputFiles) {
      var digests = inputFiles.Aggregate(new Dictionary<string, Digest>(),
                                         AddFileDigest);
      return new ReadOnlyDictionary<string, Digest>(digests);
    }

    private static Dictionary<string, Digest> AddFileDigest(Dictionary<string, Digest> digests,
                                                            string file) {
      digests.Add(Path.GetFileName(file),
                  new Digest(File.GetLastWriteTime(file), GetMd5Hash(file)));
      return digests;
    }
  }
}