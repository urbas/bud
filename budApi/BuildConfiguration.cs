using System;
using System.Collections.Immutable;

namespace Bud {
  public class BuildConfiguration {
    public static readonly ImmutableList<Setting> Start = ImmutableList.Create<Setting>();

    public string ProjectBaseDir { private set; get; }

    public BuildConfiguration(string projectBaseDir) {
      ProjectBaseDir = projectBaseDir;
    }
  }
}