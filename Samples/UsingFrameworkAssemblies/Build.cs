//!reference System.Core

using System.Linq;

public class HelloWorld {
  public static void Main(string[] args) {
    var shouts = args.Select(str => str.ToUpper());
    System.Console.Write($"Hello, {string.Join(" ", shouts)}!");
  }
}
