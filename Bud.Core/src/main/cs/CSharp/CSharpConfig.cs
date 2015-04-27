using System.Collections.Generic;
using System.Threading.Tasks;
using Bud.Build;
using NuGet;

namespace Bud.CSharp {
  public static class CSharpConfig {
    public static Framework GetTargetFramework(this IConfig config, Key buildTarget) {
      return config.Evaluate(buildTarget / Cs.TargetFramework);
    }

    public static string GetCSharpOutputAssemblyDir(this IConfig context, Key buildTarget) {
      return context.Evaluate(buildTarget / Cs.OutputAssemblyDir);
    }

    public static string GetCSharpOutputAssemblyName(this IConfig context, Key buildTarget) {
      return context.Evaluate(buildTarget / Cs.OutputAssemblyName);
    }

    public static string GetRootNamespace(this IConfig context, Key buildTarget) {
      return context.Evaluate(buildTarget / Cs.RootNamespace);
    }

    public static string GetCSharpOutputAssemblyFile(this IConfig context, Key buildTarget) {
      return context.Evaluate(buildTarget / Cs.OutputAssemblyFile);
    }

    public static AssemblyType GetCSharpAssemblyType(this IConfig context, Key project) {
      return context.Evaluate(project / Cs.AssemblyType);
    }

    public static Task CSharpBuild(this IContext context, Key project) {
      return context.Evaluate(project / BuildKeys.Main / Cs.CSharp / BuildKeys.Build);
    }

    public static IEnumerable<IPackageAssemblyReference> GetAssemblyReferences(this IConfig config, Key buildTarget) {
      return config.Evaluate(buildTarget / Cs.AssemblyReferences);
    }

    public static IEnumerable<string> GetAssemblyReferencePaths(this IConfig config, Key buildTarget) {
      return config.Evaluate(buildTarget / Cs.AssemblyReferencePaths);
    }

    public static string GetDistDir(this IConfig context, Key buildTarget) {
      return context.Evaluate(buildTarget / Cs.DistDir);
    }
  }
}