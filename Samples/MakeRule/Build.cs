//!reference Bud.Make

using System.IO;
using static Bud.Make;

class Build {
  static void Main(string[] args)
    => DoMake("foo.out", Rule("foo.out", RemoveSpaces, "foo.in"));

  static void RemoveSpaces(string inputFile, string outputFile)
    => File.WriteAllText(outputFile, File.ReadAllText(inputFile).Replace(" ", ""));
}