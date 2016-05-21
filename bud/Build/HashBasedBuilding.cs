using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Bud.Build {
  public class HashBasedBuilding {
    /// <summary>
    ///   Calculates the hash of <paramref name="input"/> files and invokes <paramref name="outputGenerator"/>
    ///   only if the hash is different from the one generated previously.
    /// </summary>
    /// <param name="outputGenerator">this function actually produces the output.</param>
    /// <param name="output">the path of the expected output.</param>
    /// <param name="input">the files from which the <paramref name="outputGenerator"/> should generate the output.</param>
    public static void Build(IOutputGenerator outputGenerator,
                             string output,
                             IImmutableList<string> input) {
      var inputHashes = output + ".input_digest";
      var digest = Digest(input);
      if (!File.Exists(output)) {
        outputGenerator.Generate(output, input);
        File.WriteAllBytes(inputHashes, digest);
      } else {
        if (!File.Exists(inputHashes) || !File.ReadAllBytes(inputHashes).SequenceEqual(digest)) {
          outputGenerator.Generate(output, input);
          File.WriteAllBytes(inputHashes, digest);
        }
      }
    }

    private static byte[] Digest(IEnumerable<string> files) {
      var sha256 = SHA256.Create();
      sha256.Initialize();
      var buffer = new byte[1 << 15];
      foreach (var file in files) {
        using (var fileStream = File.OpenRead(file)) {
          int readBytes;
          do {
            readBytes = fileStream.Read(buffer, 0, buffer.Length);
            sha256.TransformBlock(buffer, 0, readBytes, buffer, 0);
          } while (readBytes == buffer.Length);
        }
      }
      sha256.TransformFinalBlock(buffer, 0, 0);
      return sha256.Hash;
    }
  }
}