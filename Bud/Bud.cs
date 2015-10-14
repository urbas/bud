using System;
using Bud;

public class Build : IBuild {
  public Conf Init(string dir) {
    return Conf.Empty.Init("hello", configs => {
      Console.WriteLine($"Hello world in {dir}!");
      return 0;
    });
  }
}