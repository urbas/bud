using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Bud.Build;

namespace Bud.Resources {
  public class ResourcesBuildTarget : BuildTargetPlugin {
    public static readonly Key Resources = Key.Define("resources", "The scope of the resources build target.");

    public ResourcesBuildTarget(Key buildScope, params Setup[] extraBuildTargetSetup) : base(buildScope, Resources, extraBuildTargetSetup) {}

    protected override Settings BuildTargetSetup(Settings projectSettings) {
      return projectSettings.Add(BuildTargetKeys.SourceFiles.InitSync(FindResourceFiles));
    }

    private static IEnumerable<string> FindResourceFiles(IContext context, Key buildTarget) {
      var sourceDirectory = Path.Combine(context.GetBaseDir(buildTarget));
      if (Directory.Exists(sourceDirectory)) {
        return Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories);
      }
      return ImmutableList<string>.Empty;
    }
  }
}