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
    public static readonly TaskKey<Unit> CSharpBuild = BuildPlugin.Build.In(CSharp);

    public static Settings AddCSharpSupport(this ScopedSettings scopedSettings) {
      return Initialise(scopedSettings)
        .EnsureInitialized(CSharpBuild.In(scopedSettings.Scope), buildConfig => MonoCompiler.Compile(buildConfig, scopedSettings.Scope));
    }

    private static Settings Initialise(Settings existingSettings) {
      return existingSettings.AddBuildSupport()
        .EnsureInitialized(CSharpBuild, MonoCompiler.Compile)
        .AddDependencies(BuildPlugin.Build, CSharpBuild);
    }

    public static string GetCSharpSourceDir(this BuildConfiguration buildConfiguration, ISettingKey project) {
      return Path.Combine(buildConfiguration.GetBaseDir(project), "src", "main", "cs");
    }

    public static string GetCSharpOutputAssemblyFile(this BuildConfiguration buildConfiguration, ISettingKey project) {
      return Path.Combine(buildConfiguration.GetOutputDir(project), ".net-4.5", "main", "debug", "bin", "program.exe");
    }
  }

  public static class MonoCompiler {

    public static Unit Compile(BuildConfiguration buildConfiguration) {
      // TODO: evaluate per-project builds in parallel.
      foreach (var project in buildConfiguration.Evaluate(ProjectPlugin.ListOfProjects)) {
        buildConfiguration.Evaluate(CSharpPlugin.CSharpBuild.In(project));
      }
      return Unit.Instance;
    }

    public static Unit Compile(BuildConfiguration buildConfiguration, ISettingKey project) {
      var sourceDirectory = buildConfiguration.GetCSharpSourceDir(project);
      var outputFile = buildConfiguration.GetCSharpOutputAssemblyFile(project);
      var sourceFiles = Directory.EnumerateFiles(sourceDirectory);
      if (Directory.Exists(sourceDirectory) && sourceFiles.Any()) {
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
        var cSharpCompiler = "/usr/bin/mcs";
        var exitCode = ProcessBuilder.Executable(cSharpCompiler).WithArgument("-out:" + outputFile).WithArguments(sourceFiles).Start(Console.Out, Console.Error);
        if (exitCode != 0) {
          throw new Exception("Compilation failed.");
        }
      }
      return Unit.Instance;
    }
  }
}

