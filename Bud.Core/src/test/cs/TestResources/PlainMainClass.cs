using System;

namespace Bud.TestResources {
  public class PlainMainClass {
    public static void Main(string[] args) {
      Console.WriteLine("This is a plain main class. Args: " + string.Join(", ", args));
    }
  }
}