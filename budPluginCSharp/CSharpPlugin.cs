using Bud.Plugins.CSharp.Compiler;
using Bud.Plugins.Projects;
using System.IO;
using Bud.Util;
using Bud.Plugins.Build;

namespace Bud.Plugins.CSharp {

  public static class CSharpPlugin {
    public static readonly Scope CSharp = new Scope("CSharp");
    public static readonly TaskKey<Unit> CSharpBuild = BuildPlugin.Build.In(CSharp);

    public static Settings BuildsCSharp(this Settings settings) {
      return settings
        .AddBuildSupport()
        .AddCSharpBuildSupport()
        .InitOrKeep(CSharpBuild.In(settings.CurrentScope), ctxt => MonoCompiler.CompileProject(ctxt, settings.CurrentScope))
        .AddDependencies(CSharpBuild, CSharpBuild.In(settings.CurrentScope));
    }

    public static Settings AddCSharpBuildSupport(this Settings existingSettings) {
      return existingSettings
        .InitOrKeep(CSharpBuild, TaskUtils.NoOpTask)
        .AddDependencies(BuildPlugin.Build, CSharpBuild);
    }

    public static string GetCSharpSourceDir(this EvaluationContext context, Scope project) {
      return Path.Combine(context.GetBaseDir(project), "src", "main", "cs");
    }

    public static string GetCSharpOutputAssemblyFile(this EvaluationContext context, Scope project) {
      return Path.Combine(context.GetOutputDir(project), ".net-4.5", "main", "debug", "bin", "program.exe");
    }
  }
}

