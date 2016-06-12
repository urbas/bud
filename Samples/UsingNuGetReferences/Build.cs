//!nuget Newtonsoft.Json 8.0.3

using Newtonsoft.Json;

class HelloWorld {
  static void Main(string[] args)
    => System.Console.Write(JsonConvert.SerializeObject(new object[]{42, "answer"}));
}
