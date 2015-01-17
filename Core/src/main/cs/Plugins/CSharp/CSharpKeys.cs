using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.Plugins.CSharp {
  public static class CSharpKeys {
    public static readonly Key CSharp = new Key("cs");
    public static readonly ConfigKey<AssemblyType> AssemblyType = new ConfigKey<AssemblyType>("assemblyType");
    public static readonly ConfigKey<string> OutputAssemblyDir = new ConfigKey<string>("outputAssemblyDir");
    public static readonly ConfigKey<string> OutputAssemblyName = new ConfigKey<string>("outputAssemblyName");
    public static readonly ConfigKey<string> OutputAssemblyFile = new ConfigKey<string>("outputAssemblyFile");
    public static readonly TaskKey<IEnumerable<string>> SourceFiles = new TaskKey<IEnumerable<string>>("sourceFiles");
    public static readonly TaskKey<ImmutableList<string>> CollectReferencedAssemblies = new TaskKey<ImmutableList<string>>("collectReferencedAssemblies");
    public static readonly TaskKey<Unit> Dist = new TaskKey<Unit>("dist");
  }
}