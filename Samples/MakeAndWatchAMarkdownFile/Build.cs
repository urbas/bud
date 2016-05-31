//!reference Markdown 1.14.5

using MarkdownSharp;
using System.IO;

class Build {
  public static void Main(string[] args)
    => System.Console.Write(new Markdown().Transform(File.ReadAllText(args[0])));
}