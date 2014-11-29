using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Bud.Cli;
using Bud.Plugins;
using Bud.SettingsConstruction.Ops;
using System.Collections.Immutable;
using Bud.SettingsConstruction;

namespace Bud.Plugins.CSharp {

  public static class CSharpPlugin {
    public static readonly SettingKey CSharp = new SettingKey("CSharp");
    public static readonly TaskKey<Unit> Build = BuildPlugin.Build.In(CSharp);

    public static Settings AddCSharpSupport(this ScopedSettings scopedSettings) {
      var configKey = ProjectPlugin.BaseDir.In(scopedSettings.Scope);
      return Initialise(scopedSettings)
        .EnsureInitialized(Build.In(scopedSettings.Scope), configKey, MonoCompiler.Compile);
    }

    private static Settings Initialise(Settings existingSettings) {
      return existingSettings.AddBuildSupport()
        .EnsureInitialized(Build, MonoCompiler.Compile);
    }
  }

  public static class MonoCompiler {

    public static Unit Compile(BuildConfiguration buildConfiguration) {
      foreach (var project in buildConfiguration.Evaluate(ProjectPlugin.ListOfProjects)) {
        var baseDir = buildConfiguration.Evaluate(ProjectPlugin.BaseDir.In(project));
        Compile(baseDir);
      }
      return Unit.Instance;
    }

    public static Unit Compile(string baseDir) {
      var sourceDirectory = GetSourceDirectory(baseDir);
      var sourceFiles = Directory.EnumerateFiles(sourceDirectory);
      if (Directory.Exists(sourceDirectory) && sourceFiles.Any()) {
        var outputFile = GetDefaultOutputFile(baseDir);
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
        var cSharpCompiler = "/usr/bin/mcs";
        var exitCode = Processes.Execute(cSharpCompiler).AddArgument("-out:" + outputFile).AddArguments(sourceFiles).Execute(Console.Out, Console.Error);
        if (exitCode != 0) {
          throw new Exception("Compilation failed.");
        }
      }
      return Unit.Instance;
    }

    public static string GetSourceDirectory(string projectBaseDir) {
      return Path.Combine(projectBaseDir, "src", "main", "cs");
    }

    public static string GetDefaultOutputFile(string projectBaseDir) {
      var budOutputDirectory = BudPaths.GetOutputDirectory(projectBaseDir);
      return Path.Combine(budOutputDirectory, ".net-4.5", "main", "debug", "bin", "program.exe");
    }
  }
}

