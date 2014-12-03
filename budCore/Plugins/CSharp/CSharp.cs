using System;
using System.IO;
using Bud.Plugins.Build;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bud.Plugins.CSharp {
  public static class CSharp {
    public static Settings Project(string id, string baseDir) {
      return Bud.Plugins.Projects.Project.New(id, baseDir)
        .Add(CSharpPlugin.Instance);
    }

    public static string GetCSharpSourceDir(this EvaluationContext context, Scope project) {
      return Path.Combine(context.GetBaseDir(project), "src", "main", "cs");
    }

    public static Task<IEnumerable<string>> GetCSharpSources(this EvaluationContext context, Scope project) {
      return context.Evaluate(CSharpKeys.SourceFiles.In(project));
    }

    public static string GetCSharpOutputAssemblyFile(this EvaluationContext context, Scope project) {
      return Path.Combine(context.GetOutputDir(project), ".net-4.5", "main", "debug", "bin", "program.exe");
    }
  }
}

