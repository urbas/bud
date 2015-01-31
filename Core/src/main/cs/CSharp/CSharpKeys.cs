using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.CSharp {
  public static class CSharpKeys {
    public static readonly Key CSharp = new Key("cs");
    public static readonly ConfigKey<Framework> TargetFramework = new ConfigKey<Framework>("targetFramework");
    public static readonly ConfigKey<AssemblyType> AssemblyType = new ConfigKey<AssemblyType>("assemblyType");
    public static readonly ConfigKey<string> OutputAssemblyDir = new ConfigKey<string>("outputAssemblyDir");
    public static readonly ConfigKey<string> OutputAssemblyName = new ConfigKey<string>("outputAssemblyName");
    public static readonly ConfigKey<string> OutputAssemblyFile = new ConfigKey<string>("outputAssemblyFile");
    public static readonly TaskKey<IEnumerable<string>> SourceFiles = new TaskKey<IEnumerable<string>>("sourceFiles");
    public static readonly ConfigKey<IEnumerable<string>> ReferencedAssemblies = new ConfigKey<IEnumerable<string>>("referencedAssemblies");
    public static readonly TaskKey Dist = new TaskKey("dist");
  }
}