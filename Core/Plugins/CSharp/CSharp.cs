using System;
using System.IO;
using Bud.Plugins.Build;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.Plugins.CSharp {
  public static class CSharp {
    public static Settings Project(string id, string baseDir) {
      return Bud.Plugins.Projects.Project.New(id, baseDir)
        .Add(CSharpPlugin.Instance);
    }

    public static Settings LibraryProject(string id, string baseDir) {
      var project = Project(id, baseDir);
      return project.Modify(CSharpKeys.AssemblyType.In(project.CurrentScope), assemblyType => AssemblyType.Library);
    }

    public static string GetCSharpSourceDir(this EvaluationContext context, Scope project) {
      return Path.Combine(context.GetBaseDir(project), "src", "main", "cs");
    }

    public static Task<IEnumerable<string>> GetCSharpSources(this EvaluationContext context, Scope project) {
      return context.Evaluate(CSharpKeys.SourceFiles.In(project));
    }

    public static string GetCSharpOutputAssemblyDir(this EvaluationContext context, Scope project) {
      return context.Evaluate(CSharpKeys.OutputAssemblyDir.In(project));
    }

    public static string GetCSharpOutputAssemblyName(this EvaluationContext context, Scope project) {
      return context.Evaluate(CSharpKeys.OutputAssemblyName.In(project));
    }

    public static string GetCSharpOutputAssemblyFile(this EvaluationContext context, Scope project) {
      return context.Evaluate(CSharpKeys.OutputAssemblyFile.In(project));
    }

    public static Task<ImmutableList<string>> CollectCSharpReferencedAssemblies(this EvaluationContext context, Scope project) {
      return context.Evaluate(CSharpKeys.CollectReferencedAssemblies.In(project));
    }

    public static AssemblyType GetCSharpAssemblyType(this EvaluationContext context, Scope project) {
      return context.Evaluate(CSharpKeys.AssemblyType.In(project));
    }

    public static Task CSharpBuild(this EvaluationContext context, Scope project) {
      return context.Evaluate(CSharpKeys.Build.In(project));
     }

    public static string GetAssemblyFileExtension(this EvaluationContext context, Scope project) {
      switch (context.GetCSharpAssemblyType(project)) {
        case AssemblyType.Exe:
          return "exe";
        case AssemblyType.Library:
          return "dll";
        case AssemblyType.WinExe:
          return "exe";
        case AssemblyType.Module:
          return "module";
        default:
          throw new ArgumentException("Unsupported assembly type.");
      }
    }
  }
}

