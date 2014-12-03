using System;
using Bud.Plugins.Build;

namespace Bud.Plugins.CSharp {
  public static class CSharpKeys {
    public static readonly Scope CSharp = new Scope("CSharp");
    public static readonly TaskKey<Unit> Build = BuildKeys.Build.In(CSharp);
  }
}

