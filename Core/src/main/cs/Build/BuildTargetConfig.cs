using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bud.Build {
  public static class BuildTargetConfig {
    public async static Task<IEnumerable<string>> GetSourceFiles(this IContext context, Key buildTarget) {
      var sourceFilesKey = buildTarget / BuildTargetKeys.SourceFiles;
      return context.IsTaskDefined(sourceFilesKey) ? await context.Evaluate(sourceFilesKey) : ImmutableList<string>.Empty;
    }
  }
}