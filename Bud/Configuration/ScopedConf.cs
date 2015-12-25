using System.Collections.Immutable;
using Bud.V1;
using Bud.Optional;

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

    public Optional<T> TryGet<T>(Key<T> key)
      => CachingConf.TryGet(key, RawTryGet);

    private Optional<T> RawTryGet<T>(Key<T> key)
      => Conf.TryGet<T>(Keys.InterpretFromScope(key.Id, Scope));

    public static IConf MakeScoped(ImmutableList<string> scope, IConf conf)
      => scope.IsEmpty ? conf : new ScopedConf(scope, conf);
  }
}