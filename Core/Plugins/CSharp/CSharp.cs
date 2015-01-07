using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Plugins.Build;
using Bud.Plugins.Projects;
using Bud.Plugins.Dependencies;

namespace Bud.Plugins.CSharp {
  public static class CSharp {
    public static Settings CSharpProject(this Settings build, string id, string baseDir, IPlugin plugin = null) {
      return build.AddProject(id, baseDir, CSharpPlugin.Instance.With(plugin));
    }

    public static Settings LibraryProject(this Settings build, string id, string baseDir, IPlugin plugin = null) {
      return build.CSharpProject(id, baseDir, Plugin.Create(SetLibraryAssemblyType).With(plugin));
    }

    public static IPlugin Dependency(string packageName, string packageVersion = null) {
      var projectKey = Project.ProjectKey(packageName);
      return Dependencies.Dependencies.AddDependency(
        CSharpKeys.CSharp,
        new InternalDependency(projectKey, CSharpKeys.Build.In(projectKey)),
        new ExternalDependency(packageName, packageVersion),
        shouldUseInternalDependency: context => context.IsConfigDefined(CSharpKeys.OutputAssemblyFile.In(projectKey))
      );
    }

    public static string GetCSharpSourceDir(this IContext context, Key project) {
      return Path.Combine(context.GetBaseDir(project), "src", "main", "cs");
    }

    public static Task<IEnumerable<string>> GetCSharpSources(this IContext context, Key project) {
      return context.Evaluate(CSharpKeys.SourceFiles.In(project));
    }

    public static string GetCSharpOutputAssemblyDir(this IConfig context, Key project) {
      return context.Evaluate(CSharpKeys.OutputAssemblyDir.In(project));
    }

    public static string GetCSharpOutputAssemblyName(this IConfig context, Key project) {
      return context.Evaluate(CSharpKeys.OutputAssemblyName.In(project));
    }

    public static string GetCSharpOutputAssemblyFile(this IConfig context, Key project) {
      return context.Evaluate(CSharpKeys.OutputAssemblyFile.In(project));
    }

    public static Task<ImmutableList<string>> CollectCSharpReferencedAssemblies(this IContext context, Key project) {
      return context.Evaluate(CSharpKeys.CollectReferencedAssemblies.In(project));
    }

    public static AssemblyType GetCSharpAssemblyType(this IConfig context, Key project) {
      return context.Evaluate(CSharpKeys.AssemblyType.In(project));
    }

    public static Task CSharpBuild(this IContext context, Key project) {
      return context.Evaluate(CSharpKeys.Build.In(project));
     }

    public static string GetAssemblyFileExtension(this IConfig context, Key project) {
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

    private static Settings SetLibraryAssemblyType(Settings existingSettings, Key key) {
      return existingSettings.Modify(CSharpKeys.AssemblyType.In(key), assemblyType => AssemblyType.Library);
    }
  }
}

