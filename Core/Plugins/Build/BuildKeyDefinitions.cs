using System;
using System.Threading.Tasks;

namespace Bud.Plugins.Build {
  public interface IScopedLanguageBuild {
    Key Language { get; }
    Key Scope { get; }
    Task<Unit> Build(IContext context);
  }
}