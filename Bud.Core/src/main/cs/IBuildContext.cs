using Bud.Logging;

namespace Bud {
  public interface IBuildContext {
    IContext Context { get; }
    IConfig Config { get; }
    Settings Settings { get; }
    ILogger Logger { get; }
    IBuildContext WithSettings(Settings newSettings);
    IBuildContext Reset();
  }
}