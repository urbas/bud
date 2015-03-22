using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bud.Build;
using Bud.BuildDefinition;
using Bud.CSharp;
using NuGet;

namespace Bud.Projects {
  public static class ProjectsSettings {
    public static Settings Project(this Settings settings, string id, string baseDir, params Setup[] setups) {
      return settings.Add(ProjectPlugin.Project(id, baseDir, setups));
    }

    public static Settings BudPlugin(this Settings settings, string id, string baseDir, params Setup[] setups) {
      return settings.Add(ProjectPlugin.BudPlugin(id, baseDir, setups));
    }

    public static Settings BuildDefinition(this Settings settings, params Setup[] setups) {
      return settings.AddIn(ProjectPlugin.ProjectKey(BuildDefinitionPlugin.BuildDefinitionProjectId) / BuildKeys.Main / CSharpKeys.CSharp, setups);
    }

    public static Settings Version(this Settings settings, string version) {
      return settings.Add(ProjectKeys.Version.Modify(SemanticVersion.Parse(version)));
    }
  }
}