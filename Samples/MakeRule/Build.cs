//!reference Bud.Make

using System.IO;
using Bud;

class Build {
  static void Main(string[] args)
    => Make.Execute("foo.out",
                    Make.Rule("foo.out", RemoveSpaces, "foo.in"));

  static void RemoveSpaces(string inputFile, string outputFile) {
    var inputFileContent = File.ReadAllText(inputFile);
    var outputFileContent = inputFileContent.Replace(" ", "");
    File.WriteAllText(outputFile, outputFileContent);
  }
}