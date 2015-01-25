using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.Build;

namespace Bud.CSharp {
  public static class CSharpConfig {
    public static Task<IEnumerable<string>> GetCSharpSources(this IContext context, Key project) {
      return context.Evaluate(CSharpKeys.SourceFiles.In(project));
    }

    public static Framework GetTargetFramework(this IConfig config, Key buildTarget) {
      return config.Evaluate(CSharpKeys.TargetFramework.In(buildTarget));
    }

    public static string GetCSharpOutputAssemblyDir(this IConfig context, Key key) {
      return context.Evaluate(CSharpKeys.OutputAssemblyDir.In(key));
    }

    public static string GetCSharpOutputAssemblyName(this IConfig context, Key key) {
      return context.Evaluate(CSharpKeys.OutputAssemblyName.In(key));
    }

    public static string GetCSharpOutputAssemblyFile(this IConfig context, Key key) {
      return context.Evaluate(CSharpKeys.OutputAssemblyFile.In(key));
    }

    public static Task<ImmutableList<string>> CollectCSharpReferencedAssemblies(this IContext context, Key project) {
      return context.Evaluate(CSharpKeys.CollectReferencedAssemblies.In(project));
    }

    public static AssemblyType GetCSharpAssemblyType(this IConfig context, Key project) {
      return context.Evaluate(CSharpKeys.AssemblyType.In(project));
    }

    public static Task CSharpBuild(this IContext context, Key project) {
      return context.Evaluate(BuildUtils.BuildTaskKey(project, BuildKeys.Main, CSharpKeys.CSharp));
    }
  }
}