//!reference System.Core

using System.Linq;

class HelloWorld {
  static void Main(string[] args) {
    var shouts = args.Select(str => str.ToUpper());
    System.Console.Write($"Hello, {string.Join(" ", shouts)}!");
  }
}
