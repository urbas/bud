using System;

namespace Bud {
  public class Bud : IBuild {
    public Configs Init(string dir) {
      return Configs.Empty.Init("hello", configs => {
        Console.WriteLine($"Hello world in {dir}!");
        return 0;
      });
    }
  }
}