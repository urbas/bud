using System;
using System.IO;
using System.Diagnostics;

namespace Bud.Plugin.CSharp {
  public class CSharpPlugin {

    public static void Compile(BuildConfiguration buildConfiguration) {
      var sourceDirectory = GetSourceDirectory(buildConfiguration.ProjectBaseDir);
      var sourceFiles = Directory.EnumerateFiles(sourceDirectory);
      var outputFile = GetOutputFile(buildConfiguration.ProjectBaseDir);
      Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
      var cSharpCompiler = "/usr/bin/mcs";
      using (var process = new Process()) {
        var compilerArguments = CliUtils.Argument("-out:" + outputFile).Add(sourceFiles).ToString();
        process.StartInfo.FileName = cSharpCompiler;
        process.StartInfo.Arguments = compilerArguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();
        Console.WriteLine(process.StandardOutput.ReadToEnd());
        Console.Error.WriteLine(process.StandardError.ReadToEnd());
        process.WaitForExit();
      }
    }

    private static string GetSourceDirectory(string projectBaseDir) {
      return Path.Combine(projectBaseDir, "src", "main", "cs");
    }

    private static string GetOutputFile(string projectBaseDir) {
      var budOutputDirectory = BudPaths.GetOutputDirectory(projectBaseDir);
      return Path.Combine(budOutputDirectory, ".net-4.5", "main", "debug", "bin", "program.exe");
    }
  }
}

