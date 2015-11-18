using System.Collections.Immutable;

namespace Bud.Configuration {
  public class SubscopingConf : IConf {
    private ImmutableList<string> Scope { get; }
    private IConf Conf { get; }

    public SubscopingConf(ImmutableList<string> scope, IConf conf) {
      Scope = scope;
      Conf = conf;
    }

    public T Get<T>(Key<T> key)
      => Conf.Get<T>(Keys.InterpretFromScope(key.Id, Scope));

    public static IConf MakeScoped(ImmutableList<string> scope, IConf conf)
      => scope.IsEmpty ? conf : new SubscopingConf(scope, conf);
  }
}