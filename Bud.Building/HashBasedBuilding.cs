using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Bud.Building {
  public class HashBasedBuilding {
    private static readonly byte[] DefaultSalt = new byte[0];

    /// <summary>
    ///   Calculates the hash of <paramref name="input" /> files and invokes <paramref name="outputGenerator" />
    ///   only if the hash is different from the one generated previously.
    /// </summary>
    /// <param name="outputGenerator">this function actually produces the output.</param>
    /// <param name="output">the path of the expected output.</param>
    /// <param name="input">the files from which the <paramref name="outputGenerator" /> should generate the output.</param>
    /// <remarks>
    ///   Note that the order of input files is significant. Different order of input files will produce
    ///   a different hash. If your <paramref name="outputGenerator" /> is order-invariant, we suggest you
    ///   order the input before invoking this function.
    /// </remarks>
    public static void Build(IOutputGenerator outputGenerator,
                             string output,
                             IImmutableList<string> input) {
      Build(outputGenerator, output, input, output + ".input_hash", DefaultSalt);
    }

    /// <summary>
    ///   Similar to <see cref="Build(IOutputGenerator,string,IImmutableList{string})" /> except
    ///   you can provide your own <paramref name="inputHashFile" />.
    /// </summary>
    /// <param name="outputGenerator"></param>
    /// <param name="output"></param>
    /// <param name="input"></param>
    /// <param name="inputHashFile">
    ///   this file contains the has of all <paramref name="input" />
    ///   file combined. This file is updated each time <paramref name="output" /> is generated. If this
    ///   file does not exist, or if the content of this file does not match the hash
    ///   of the <paramref name="input" />, then <paramref name="output" /> is regenerated.
    /// </param>
    /// <param name="salt">
    ///   this salt is used when calculating the input hash. The intended
    ///   use of this salt is as the hash of the generator. For example, the salt could be
    ///   the version of the generator and the parameters of the generator.
    /// </param>
    public static void Build(IOutputGenerator outputGenerator,
                             string output,
                             IImmutableList<string> input,
                             string inputHashFile,
                             byte[] salt) {
      var digest = Digest(input, salt);
      if (!File.Exists(output)) {
        outputGenerator.Generate(output, input);
        File.WriteAllBytes(inputHashFile, digest);
      } else {
        if (!File.Exists(inputHashFile) || !File.ReadAllBytes(inputHashFile).SequenceEqual(digest)) {
          outputGenerator.Generate(output, input);
          File.WriteAllBytes(inputHashFile, digest);
        }
      }
    }

    private static byte[] Digest(IEnumerable<string> files, byte[] salt) {
      var digest = MD5.Create();
      digest.Initialize();
      digest.TransformBlock(salt, 0, salt.Length, salt, 0);
      var buffer = new byte[1 << 15];
      foreach (var file in files) {
        using (var fileStream = File.OpenRead(file)) {
          int readBytes;
          do {
            readBytes = fileStream.Read(buffer, 0, buffer.Length);
            digest.TransformBlock(buffer, 0, readBytes, buffer, 0);
          } while (readBytes == buffer.Length);
        }
      }
      digest.TransformFinalBlock(buffer, 0, 0);
      return digest.Hash;
    }
  }
}