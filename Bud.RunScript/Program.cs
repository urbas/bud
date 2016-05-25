using System.IO;
using Bud.Scripting;

namespace Bud.RunScript {
  public class Program {
    public static void Main(string[] args)
      => ScriptRunner.Run(Path.Combine(Directory.GetCurrentDirectory(), "Build.cs"),
                          args,
                          Directory.GetCurrentDirectory());
  }
}