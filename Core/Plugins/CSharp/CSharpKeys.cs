using System;
using Bud.Plugins.Build;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud.Plugins.CSharp {
  public static class CSharpKeys {
    public static readonly Key CSharp = new Key("CSharp");
    public static readonly ConfigKey<AssemblyType> AssemblyType = new ConfigKey<AssemblyType>("AssemblyType").In(CSharp);
    public static readonly ConfigKey<string> OutputAssemblyDir = new ConfigKey<string>("OutputAssemblyDir").In(CSharp);
    public static readonly ConfigKey<string> OutputAssemblyName = new ConfigKey<string>("OutputAssemblyName").In(CSharp);
    public static readonly ConfigKey<string> OutputAssemblyFile = new ConfigKey<string>("OutputAssemblyFile").In(CSharp);
    public static readonly TaskKey<IEnumerable<string>> SourceFiles = new TaskKey<IEnumerable<string>>("SourceFiles").In(CSharp);
    public static readonly TaskKey<ImmutableList<string>> CollectReferencedAssemblies = new TaskKey<ImmutableList<string>>("CollectReferencedAssemblies").In(CSharp);
    public static readonly TaskKey<Unit> Build = BuildKeys.Build.In(CSharp);
  }
}

