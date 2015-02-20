using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.CSharp {
  public static class CSharpKeys {
    public static readonly Key CSharp = Key.Define("cs");
    public static readonly ConfigKey<Framework> TargetFramework = ConfigKey<Framework>.Define("targetFramework");
    public static readonly ConfigKey<AssemblyType> AssemblyType = ConfigKey<AssemblyType>.Define("assemblyType");
    public static readonly ConfigKey<string> OutputAssemblyDir = ConfigKey<string>.Define("outputAssemblyDir");
    public static readonly ConfigKey<string> OutputAssemblyName = ConfigKey<string>.Define("outputAssemblyName");
    public static readonly ConfigKey<string> OutputAssemblyFile = ConfigKey<string>.Define("outputAssemblyFile");
    public static readonly TaskKey<IEnumerable<string>> SourceFiles = TaskKey<IEnumerable<string>>.Define("sourceFiles");
    public static readonly ConfigKey<IEnumerable<string>> ReferencedAssemblies = ConfigKey<IEnumerable<string>>.Define("referencedAssemblies");
    public static readonly TaskKey Dist = TaskKey.Define("dist");
  }
}