using System;
using Bud.Plugins;
using System.Collections.Immutable;
using Bud.Plugins.Projects;
using System.IO;

namespace Bud.Plugins.BuildLoading
{
  public class BuildLoadingPlugin : BudPlugin	{
    public BuildLoadingPlugin(string dirOfProjectToBeBuilt) {
    }

    public Settings ApplyTo(Settings settings, Scope scope) {
      return settings;
    }
  }

}

