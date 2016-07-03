using System.IO;

namespace Bud.Scripting.TestScripts {
  public class CreateFooFile {
    public static void Main(string[] args)
      => File.WriteAllText("foo", "42 " + args[0]);
  }
}