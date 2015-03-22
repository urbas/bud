using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Bud.Build;

namespace Bud.Resources {
  public class ResourcesBuildTarget : BuildTargetPlugin {
    public ResourcesBuildTarget(Key buildScope, params Setup[] setups) : base(buildScope, ResourcesKeys.Resources, setups) {}

    protected override Settings Setup(Settings projectSettings, Key project) {
      return projectSettings.Add(BuildTargetKeys.SourceFiles.InitSync(FindResourceFiles));
    }

    private IEnumerable<string> FindResourceFiles(IContext context, Key buildTarget) {
      var sourceDirectory = Path.Combine(context.GetBaseDir(buildTarget));
      if (Directory.Exists(sourceDirectory)) {
        return Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories);
      }
      return ImmutableList<string>.Empty;
    }
  }
}