using System;
using System.Collections.Generic;
using Bud.Build;
using Bud.Dependencies;
using Bud.Projects;
using NuGet;

namespace Bud.CSharp {
  public static class Cs {
    public static readonly Key CSharp = Key.Define("cs");
    public static readonly ConfigKey<Framework> TargetFramework = Key.Define("targetFramework");
    public static readonly ConfigKey<AssemblyType> AssemblyType = Key.Define("assemblyType");
    public static readonly ConfigKey<string> OutputAssemblyDir = Key.Define("outputAssemblyDir");
    public static readonly ConfigKey<string> OutputAssemblyName = Key.Define("outputAssemblyName");
    public static readonly ConfigKey<string> RootNamespace = Key.Define("rootNamespace");
    public static readonly ConfigKey<string> OutputAssemblyFile = Key.Define("outputAssemblyFile");
    public static readonly ConfigKey<string> DistDir = Key.Define("distDir", "The directory where Bud will place the output assembly and all the referenced assemblies.");
    public static readonly ConfigKey<IEnumerable<IPackageAssemblyReference>> AssemblyReferences = Key.Define("assemblyReferences");
    public static readonly ConfigKey<string[]> AssemblyReferencePaths = Key.Define("assemblyReferencePaths");
    public static readonly TaskKey Dist = Key.Define("dist", String.Format("Places the output assembly and all its references into the '{0}' folder.", DistDir));

    public static Setup Dependency(string packageName, string packageVersion = null) {
      var dependencyProject = Project.ProjectKey(packageName);
      var dependencyBuildTarget = dependencyProject / BuildKeys.Main / CSharp;
      return DependenciesSettings.AddDependency(
        new CSharpInternalDependency(dependencyBuildTarget),
        new ExternalDependency(packageName, packageVersion),
        context => CsBuild.IsMainBuildTargetDefined(context, dependencyBuildTarget));
    }

    public static Setup Exe(params Setup[] setups) {
      return new CsBuild(BuildKeys.Main,
                         CSharpExeRunnerPlugin.Instance,
                         setups.Merge());
    }

    public static Setup Dll(params Setup[] setups) {
      return new CsBuild(BuildKeys.Main,
                         AssemblyType.Modify(Bud.CSharp.AssemblyType.Library),
                         setups.Merge());
    }

    public static Setup Test(params Setup[] setups) => new NUnitPlugin(BuildKeys.Test, setups);
  }
}