using System;
using Urbas.Example.Foo;

public class B {
  public static void Main(string[] args) {
    Console.WriteLine("I asked A for a message, and got: " + A.Message);
    Console.WriteLine("I also know Foo. Here it is: " + new Foo().WhatIf);
    Console.WriteLine("I also know Common. Here it is: " + Common.Message);
  }
}
