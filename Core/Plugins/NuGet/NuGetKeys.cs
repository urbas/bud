using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using System.IO;
using Bud.Commander;

namespace Bud.Plugins.NuGet {
  public static class NuGetKeys {
    public static readonly ConfigKey<NuGetDependencyResolver> NuGetDependencyResolver = new ConfigKey<NuGetDependencyResolver>("NuGetDependencyResolver");
  }
}

