using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Bud.CSharp;
using Bud.Projects;
using Bud.Util;

namespace Bud.Build {
  public static class BuildTargetUtils {
    public static Key ProjectOf(Key buildTarget) {
      return buildTarget.Parent.Parent;
    }

    public static Key ScopeOf(Key buildTarget) {
      return buildTarget.Parent;
    }

    public static string PackageIdOf(Key buildTarget) {
      var projectId = ProjectOf(buildTarget).Id;
      var scope = ScopeOf(buildTarget);
      return scope.Leaf.Equals(BuildKeys.Main) ? projectId : projectId + "." + StringUtils.Capitalize(scope.Id);
    }

    public static Key LanguageOf(Key buildTarget) {
      return buildTarget;
    }

    public static Guid GuidOf(Key buildTarget) {
      var idUtf8Bytes = Encoding.UTF8.GetBytes(PackageIdOf(buildTarget));
      var idHash = new MD5CryptoServiceProvider().ComputeHash(idUtf8Bytes);
      return new Guid(idHash);
    }

    public static bool HasBuildTarget(this IContext context, Key project, Key scope, Key language) {
      return context.IsTaskDefined(project / scope / language / BuildKeys.Build);
    }

    public static IEnumerable<Key> GetAllBuildTargets(IContext context) {
      return context.GetAllProjects()
                    .Select(idToProject => idToProject.Value)
                    .SelectMany(projectKey => context.Evaluate(projectKey / BuildKeys.BuildTargets));
    }
  }
}