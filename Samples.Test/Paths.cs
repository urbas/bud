using System.IO;
using System.Reflection;

namespace Samples {
  public static class Paths {
    public static string RunScript
      => Path.Combine(TestAssemblyDir, "Bud.RunScript.exe");

    public static string SamplesDir
      => Path.Combine(TestAssemblyDir, "..", "..", "..", "Samples");

    public static string Sample(string sampleName)
      => Path.Combine(SamplesDir, sampleName);

    private static string TestAssemblyDir
      => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
  }
}