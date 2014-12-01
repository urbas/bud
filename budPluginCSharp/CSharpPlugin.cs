using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Bud.Cli;
using Bud.Plugins;
using Bud.SettingsConstruction.Ops;
using System.Collections.Immutable;
using Bud.SettingsConstruction;
using System.Threading.Tasks;
using Bud.Plugins.CSharp.Compiler;

namespace Bud.Plugins.CSharp {

  public static class CSharpPlugin {
    public static readonly SettingKey CSharp = new SettingKey("CSharp");
    public static readonly TaskKey<Unit> CSharpBuild = BuildPlugin.Build.In(CSharp);

    public static ScopedSettings BuildsCSharp(this ScopedSettings scopedSettings) {
      return scopedSettings
        .AddBuildSupport()
        .EnsureInitialized(CSharpBuild, MonoCompiler.CompileAllProjects)
        .AddDependencies(BuildPlugin.Build, CSharpBuild)
        .EnsureInitialized(CSharpBuild.In(scopedSettings.Scope), buildConfig => MonoCompiler.CompileProjects(buildConfig, scopedSettings.Scope))
        .ScopedTo(scopedSettings.Scope);
    }

    public static string GetCSharpSourceDir(this EvaluationContext context, ISettingKey project) {
      return Path.Combine(context.GetBaseDir(project), "src", "main", "cs");
    }

    public static string GetCSharpOutputAssemblyFile(this EvaluationContext context, ISettingKey project) {
      return Path.Combine(context.GetOutputDir(project), ".net-4.5", "main", "debug", "bin", "program.exe");
    }
  }
}

