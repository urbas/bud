using System;
using System.Collections.Generic;
using Bud.Util;

namespace Bud.Build {
  public static class BuildUtils {
    public static Key ProjectOf(Key buildTarget) {
      return buildTarget.Parent.Parent;
    }

    public static Key ScopeOf(Key buildTarget) {
      return buildTarget.Parent;
    }

    public static string IdOf(Key buildTarget) {
      var projectId = ProjectOf(buildTarget).Id;
      var scope = ScopeOf(buildTarget);
      return scope.Leaf.Equals(BuildKeys.Main) ? projectId : projectId + "." + StringUtils.Capitalize(scope.Id);
    }

    public static Key LanguageOf(Key buildTarget) {
      return buildTarget;
    }

    public static string LogMessage(this IConfig context, Key buildTarget, string messageFormat, params object[] formatParams) {
      return String.Format("{0}:{1}/{2}> {3}", BuildUtils.ProjectOf(buildTarget).Id, BuildUtils.ScopeOf(buildTarget).Id, BuildUtils.LanguageOf(buildTarget).Id, String.Format(messageFormat, formatParams));
    }

    public static bool HasBuildTarget(this IContext context, Key project, Key scope, Key language) {
      return context.IsTaskDefined(project / scope / language / BuildKeys.Build);
    }
  }
}