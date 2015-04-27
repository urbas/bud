using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Bud.Projects;
using Bud.Util;

namespace Bud.Build {
  public static class BuildTargetUtils {
    public static Key ProjectOf(Key buildTarget) => buildTarget.Parent.Parent;

    public static Key ScopeOf(Key buildTarget) => buildTarget.Parent;

    public static Key LanguageOf(Key buildTarget) => buildTarget;

    public static string PackageIdOf(this IConfig config, Key buildTarget) {
      string packageId;
      return config.TryEvaluate(buildTarget / BuildTargetKeys.PackageId, out packageId) ? packageId : DefaultPackageId(buildTarget);
    }

    public static Guid GuidOf(this IConfig config, Key buildTarget) {
      var idUtf8Bytes = Encoding.UTF8.GetBytes(config.PackageIdOf(buildTarget));
      var idHash = new MD5CryptoServiceProvider().ComputeHash(idUtf8Bytes);
      return new Guid(idHash);
    }

    public static bool HasBuildTarget(this IContext context, Key project, Key scope, Key language) {
      var buildTarget = project / scope / language;
      return IsBuildTargetDefined(context, buildTarget);
    }

    public static bool IsBuildTargetDefined(this IContext context, Key buildTarget) {
      return context.IsTaskDefined(buildTarget / BuildKeys.Build);
    }

    public static IEnumerable<Key> GetAllBuildTargets(IContext context) {
      return context.GetAllProjects()
                    .Select(idToProject => idToProject.Value)
                    .SelectMany(projectKey => context.Evaluate(projectKey / BuildKeys.BuildTargets));
    }

    public static string DefaultPackageId(Key project, Key buildScope) {
      return buildScope.Leaf.Equals(BuildKeys.Main) ?
        project.Id :
        project.Id + "." + StringUtils.Capitalize(buildScope.Id);
    }

    private static string DefaultPackageId(Key buildTarget) => DefaultPackageId(ProjectOf(buildTarget), ScopeOf(buildTarget));
  }
}