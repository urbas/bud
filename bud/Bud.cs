using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.CSharp;

namespace Bud {
  public static class Bud {

    public static BuildConfiguration Load(string path) {
      return ProjectPlugin.Project(path, path).AddCSharpSupport().End();
    }

  }
}

