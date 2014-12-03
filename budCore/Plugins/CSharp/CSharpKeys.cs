using System;
using Bud.Plugins.Build;
using System.Collections.Generic;

namespace Bud.Plugins.CSharp {
  public static class CSharpKeys {
    public static readonly Scope CSharp = new Scope("CSharp");
    public static readonly TaskKey<IEnumerable<string>> SourceFiles = new TaskKey<IEnumerable<string>>("SourceFiles").In(CSharp);
    public static readonly TaskKey<Unit> Build = BuildKeys.Build.In(CSharp);
  }
}

