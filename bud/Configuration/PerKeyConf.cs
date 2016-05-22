using System.Collections.Immutable;
using Bud.V1;

namespace Bud.Configuration {
  internal class PerKeyConf : IConf {
    internal PerKeyConf(IConf conf, IImmutableList<string> dir, Key key) {
      Dir = dir;
      Conf = conf;
      Key = key;
      Cache = new ConfCache();
    }

    public Key Key { get; }
    public Option<T> TryGet<T>(Key<T> key) => Cache.TryGet(key, RawTryGet);
    private Option<T> RawTryGet<T>(Key<T> key) => Conf.TryGet<T>(Keys.InterpretFromDir(key.Id, Dir));
    private IImmutableList<string> Dir { get; }
    private IConf Conf { get; }
    private ConfCache Cache { get; }
  }
}