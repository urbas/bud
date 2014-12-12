using System;
using System.Collections.Immutable;

public class Program
{
  public static void Main()
  {
    var list = ImmutableList<string>.Empty;
    list.Add ("Foo");
    Console.WriteLine(list);
  }
}