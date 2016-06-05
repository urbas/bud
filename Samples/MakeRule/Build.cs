//!reference Bud.Building
//!reference Bud.Make

using System.IO;
using Bud.Make;

class Build {
  static void Main(string[] args) {
    var rule = Make.Rule("foo.out", "foo.in", RemoveSpaces);
    Make.Execute(args, rule);
  }

  private static void RemoveSpaces(string inputFile, string outputFile) {
    var inputFileContent = File.ReadAllText(inputFile);
    var outputFileContent = inputFileContent.Replace(" ", "");
    File.WriteAllText(outputFile, outputFileContent);
  }
}