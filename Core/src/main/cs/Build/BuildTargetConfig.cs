using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bud.Build {
  public static class BuildTargetConfig {
    public static Task<IEnumerable<string>> GetSourceFiles(this IContext context, Key buildTarget) {
      return context.Evaluate(buildTarget / BuildTargetKeys.SourceFiles);
    }
  }
}