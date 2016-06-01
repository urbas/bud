//!reference Bud.Option

using Bud;

class HelloWorld {
  static void Main(string[] args) {
    System.Console.Write($"This is an option: {Option.Some(42)}!");
  }
}
