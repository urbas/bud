using System.IO;
using System.Reflection;

namespace Samples {
  public static class Paths {
    /// <summary>
    ///   The location of the Bud.RunScript executable.
    /// </summary>
    public static string RunScript
      => Path.Combine(SolutionRootDir, "CliTools", "Bud.RunScript", "bin", "Debug", "Bud.RunScript.exe");

    public static string SolutionRootDir
      => Path.Combine(TestAssemblyDir, "..", "..", "..");

    public static string SamplesDir
      => Path.Combine(SolutionRootDir, "Samples");

    public static string Sample(string sampleName)
      => Path.Combine(SamplesDir, sampleName);

    private static string TestAssemblyDir
      => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
  }
}