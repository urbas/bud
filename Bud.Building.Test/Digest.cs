using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

    private static string DigestsJson(IEnumerable<string> inputFiles) {
      var digests = inputFiles.Aggregate(ImmutableDictionary<string, Digest>.Empty,
                                         AddFileDigest);
      return JsonConvert.SerializeObject(digests);
    }

    private static ImmutableDictionary<string, Digest>
      AddFileDigest(ImmutableDictionary<string, Digest> digests,
                    string file)
      => digests.Add(Path.GetFileName(file),
                     new Digest(File.GetLastWriteTime(file), GetMd5Hash(file)));

    private static string GetMd5Hash(string file) {
      using (var md5 = MD5.Create()) {
        using (var stream = File.OpenRead(file)) {
          var computeHash = md5.ComputeHash(stream);
          return BitConverter.ToString(computeHash).Replace("-", "").ToLower();
        }
      }
    }
  }
}