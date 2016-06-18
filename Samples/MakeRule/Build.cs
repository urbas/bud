//!reference Bud.Make

using System.IO;
using Bud.Make;

class Build {
  static void Main(string[] args) {
    var rules = new [] {
      Make.Rule("foo.out", RemoveSpaces, "foo.in"),
    };

    Make.Execute(rules, "foo.out");
  }

  private static void RemoveSpaces(string inputFile, string outputFile) {
    var inputFileContent = File.ReadAllText(inputFile);
    var outputFileContent = inputFileContent.Replace(" ", "");
    File.WriteAllText(outputFile, outputFileContent);
  }
}