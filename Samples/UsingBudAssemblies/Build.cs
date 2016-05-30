//!reference Bud.Option

using Bud;

public class HelloWorld {
  public static void Main(string[] args) {
    System.Console.Write($"This is an option: {Option.Some(42)}!");
  }
}
