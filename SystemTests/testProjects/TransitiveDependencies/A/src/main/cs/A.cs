using System;
using Urbas.Example.Foo;

public class A {
  public static string Message { get { return "I am the A class! My friend is: " + new Foo().WhatIf; } }
}
