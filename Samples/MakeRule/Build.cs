//!reference Bud.Make

using System.IO;
using Bud.Make;

class Build {
  static void Main(string[] args)
    => Make.Execute(new []{"foo.out"},
                    Make.Rule("foo.out", RemoveSpaces, "foo.in"));

  private static void RemoveSpaces(string inputFile, string outputFile) {
    var inputFileContent = File.ReadAllText(inputFile);
    var outputFileContent = inputFileContent.Replace(" ", "");
    File.WriteAllText(outputFile, outputFileContent);
  }
}