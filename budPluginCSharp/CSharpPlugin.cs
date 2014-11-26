using System;
using System.IO;
using System.Diagnostics;
using Bud.Cli;

namespace Bud.Plugin.CSharp {
  public class CSharpPlugin {

    public static void Compile(BuildConfiguration buildConfiguration) {
      var sourceDirectory = GetSourceDirectory(buildConfiguration.ProjectBaseDir);
      var sourceFiles = Directory.EnumerateFiles(sourceDirectory);
      var outputFile = GetOutputFile(buildConfiguration.ProjectBaseDir);
      Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
      var cSharpCompiler = "/usr/bin/mcs";
      var exitCode = Processes.Execute(cSharpCompiler).AddArgument("-out:" + outputFile).AddArguments(sourceFiles).Execute(Console.Out, Console.Error);
      if (exitCode != 0) {
        throw new Exception("Compilation failed.");
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

