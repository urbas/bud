using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Bud.Building {
  public class HashBasedBuilder {
    private static readonly byte[] DefaultSalt = new byte[0];

    /// <summary>
    ///   Calculates the hash of <paramref name="input" /> file and invokes <paramref name="filesBuilder" />
    ///   only if the hash is different from the one generated previously.
    /// </summary>
    /// <param name="filesBuilder">
    ///   this function actually produces the output.
    ///   The first parameter to the function is the input file and the second parameter is the output file.
    /// </param>
    /// <param name="hashFile">
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
    /// <param name="input">the files from which the <paramref name="filesBuilder" /> should generate the output.</param>
    /// <param name="output">the path of the expected output.</param>
    /// <remarks>
    ///   Note that the order of input files is significant. Different order of input files will produce
    ///   a different hash. If your <paramref name="filesBuilder" /> is order-invariant, we suggest you
    ///   order the input before invoking this function.
    /// </remarks>
    public static void Build(FilesBuilder filesBuilder,
                             string hashFile,
                             byte[] salt,
                             IImmutableList<string> input, string output) {
      var digest = Md5Hasher.Digest(input, salt);
      if (!File.Exists(output)) {
        filesBuilder(input, output);
        File.WriteAllBytes(hashFile, digest);
      } else {
        if (!File.Exists(hashFile) || !File.ReadAllBytes(hashFile).SequenceEqual(digest)) {
          filesBuilder(input, output);
          File.WriteAllBytes(hashFile, digest);
        }
      }
    }

    public static void Build(FilesBuilder filesBuilder, IImmutableList<string> input, string output)
      => Build(filesBuilder, output + ".input_hash", DefaultSalt, input, output);

    public static void Build(SingleFileBuilder fileBuilder, string hashFile, byte[] salt, string input, string output)
      => Build((inputFiles, outputFile) => fileBuilder(inputFiles.First(), outputFile),
               hashFile,
               salt, ImmutableList.Create(input), output);

    public static void Build(SingleFileBuilder fileBuilder, string input, string output)
      => Build((inputFiles, outputFile) => fileBuilder(inputFiles.First(), outputFile),
               ImmutableList.Create(input), output);
  }

  internal class FuncFileGenerator {
    public Action<string, IImmutableList<string>> FileGenerator { get; }

    public FuncFileGenerator(Action<string, IImmutableList<string>> fileGenerator) {
      FileGenerator = fileGenerator;
    }

    public void Generate(string outputFile, IImmutableList<string> inputFiles)
      => FileGenerator(outputFile, inputFiles);
  }
}