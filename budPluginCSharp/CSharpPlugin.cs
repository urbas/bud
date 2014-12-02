using Bud.Plugins.CSharp.Compiler;
using Bud.Plugins.Projects;
using System.IO;

namespace Bud.Plugins.CSharp {

  public static class CSharpPlugin {
    public static readonly Scope CSharp = new Scope("CSharp");
    public static readonly TaskKey<Unit> CSharpBuild = BuildPlugin.Build.In(CSharp);

    public static ScopedSettings BuildsCSharp(this ScopedSettings scopedSettings) {
      return scopedSettings
        .EnsureInitialized(CSharpBuild, MonoCompiler.CompileProject)
        .Globally(s => s
          .AddBuildSupport()
          .EnsureInitialized(CSharpBuild, MonoCompiler.CompileAllProjects)
          .AddDependencies(BuildPlugin.Build, CSharpBuild)
        );
    }

    public static string GetCSharpSourceDir(this EvaluationContext context, Scope project) {
      return Path.Combine(context.GetBaseDir(project), "src", "main", "cs");
    }

    public static string GetCSharpOutputAssemblyFile(this EvaluationContext context, Scope project) {
      return Path.Combine(context.GetOutputDir(project), ".net-4.5", "main", "debug", "bin", "program.exe");
    }
  }
}

