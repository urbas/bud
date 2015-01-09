using System;
using Bud.Plugins.Build;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.Plugins.CSharp {
  public static class CSharpKeys {
    public static readonly Key CSharp = new Key("CSharp");
    public static readonly ConfigKey<AssemblyType> AssemblyType = new ConfigKey<AssemblyType>("assemblyType").In(CSharp);
    public static readonly ConfigKey<string> OutputAssemblyDir = new ConfigKey<string>("outputAssemblyDir").In(CSharp);
    public static readonly ConfigKey<string> OutputAssemblyName = new ConfigKey<string>("outputAssemblyName").In(CSharp);
    public static readonly ConfigKey<string> OutputAssemblyFile = new ConfigKey<string>("outputAssemblyFile").In(CSharp);
    public static readonly TaskKey<IEnumerable<string>> SourceFiles = new TaskKey<IEnumerable<string>>("sourceFiles").In(CSharp);
    public static readonly TaskKey<ImmutableList<string>> CollectReferencedAssemblies = new TaskKey<ImmutableList<string>>("collectReferencedAssemblies").In(CSharp);
    public static readonly TaskKey<Unit> Build = BuildKeys.Build.In(CSharp);
  }
}

