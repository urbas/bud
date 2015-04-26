using System;
using Urbas.Example.Foo;

public class A {
  public string Message {
    get { return new Foo().Message; }
  }
}