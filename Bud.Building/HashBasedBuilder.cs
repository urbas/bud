using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Bud.Building {
  public class HashBasedBuilder {
    private static readonly byte[] DefaultSalt = new byte[0];

    /// <summary>
    ///   Calculates the hash of <paramref name="input" /> files and invokes <paramref name="fileGenerator" />
    ///   only if the hash is different from the one generated previously.
    /// </summary>
    /// <param name="fileGenerator">this function actually produces the output.</param>
    /// <param name="output">the path of the expected output.</param>
    /// <param name="input">the files from which the <paramref name="fileGenerator" /> should generate the output.</param>
    /// <remarks>
    ///   Note that the order of input files is significant. Different order of input files will produce
    ///   a different hash. If your <paramref name="fileGenerator" /> is order-invariant, we suggest you
    ///   order the input before invoking this function.
    /// </remarks>
    public static void Build(IFileGenerator fileGenerator,
                             string output,
                             IImmutableList<string> input) {
      Build(fileGenerator, output, input, output + ".input_hash", DefaultSalt);
    }

    /// <summary>
    ///   Similar to <see cref="Build(IFileGenerator,string,IImmutableList{string})" /> except
    ///   you can provide your own <paramref name="inputHashFile" />.
    /// </summary>
    /// <param name="fileGenerator"></param>
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
    public static void Build(IFileGenerator fileGenerator,
                             string output,
                             IImmutableList<string> input,
                             string inputHashFile,
                             byte[] salt) {
      var digest = Hasher.Md5(input, salt);
      if (!File.Exists(output)) {
        fileGenerator.Generate(output, input);
        File.WriteAllBytes(inputHashFile, digest);
      } else {
        if (!File.Exists(inputHashFile) || !File.ReadAllBytes(inputHashFile).SequenceEqual(digest)) {
          fileGenerator.Generate(output, input);
          File.WriteAllBytes(inputHashFile, digest);
        }
      }
    }
  }
}