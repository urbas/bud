using System.Collections.Generic;
using NuGet;

namespace Bud.CSharp {
  public static class CSharpKeys {
    public static readonly Key CSharp = Key.Define("cs");
    public static readonly ConfigKey<Framework> TargetFramework = Key.Define("targetFramework");
    public static readonly ConfigKey<AssemblyType> AssemblyType = Key.Define("assemblyType");
    public static readonly ConfigKey<string> OutputAssemblyDir = Key.Define("outputAssemblyDir");
    public static readonly ConfigKey<string> OutputAssemblyName = Key.Define("outputAssemblyName");
    public static readonly ConfigKey<string> RootNamespace = Key.Define("rootNamespace");
    public static readonly ConfigKey<string> OutputAssemblyFile = Key.Define("outputAssemblyFile");
    public static readonly ConfigKey<IEnumerable<IPackageAssemblyReference>> AssemblyReferences = Key.Define("assemblyReferences");
    public static readonly TaskKey Dist = Key.Define("dist");
  }
}