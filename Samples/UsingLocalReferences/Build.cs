//!reference Foo.Bar/bin/Debug/Foo.Bar.dll

using Foo.Bar;

class HelloWorld {
  static void Main(string[] args)
    => System.Console.Write(Foobarization.ToFooBar("9001"));
}
