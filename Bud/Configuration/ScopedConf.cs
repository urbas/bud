using System.Collections.Immutable;

namespace Bud.Configuration {
  public class ScopedConf : IConf {
    private ImmutableList<string> Scope { get; }
    private IConf Conf { get; }
    private CachingConf CachingConf { get; }

    private ScopedConf(ImmutableList<string> scope, IConf conf) {
      Scope = scope;
      Conf = conf;
      CachingConf = new CachingConf();
    }

    public T Get<T>(Key<T> key)
      => CachingConf.Get(key, RawGet);

    private T RawGet<T>(Key<T> key)
      => Conf.Get<T>(Keys.InterpretFromScope(key.Id, Scope));

    public static IConf MakeScoped(ImmutableList<string> scope, IConf conf)
      => scope.IsEmpty ? conf : new ScopedConf(scope, conf);
  }
}