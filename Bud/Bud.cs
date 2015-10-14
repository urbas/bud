using System;

namespace Bud {
  public class Bud : IBuild {
    public Conf Init(string dir) {
      return Conf.Empty.Init("hello", configs => {
        Console.WriteLine($"Hello world in {dir}!");
        return 0;
      });
    }
  }
}