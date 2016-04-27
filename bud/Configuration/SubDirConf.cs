using System.Collections.Immutable;
using Bud.V1;
using Bud.Util;

namespace Bud.Configuration {
  internal class SubDirConf : IConf {
    private IImmutableList<string> Dir { get; }
    private IConf Conf { get; }
    private ConfCache Cache { get; }

    private SubDirConf(IImmutableList<string> dir, IConf conf) {
      Dir = dir;
      Conf = conf;
      Cache = new ConfCache();
    }

    public Option<T> TryGet<T>(Key<T> key)
      => Cache.TryGet(key, RawTryGet);

    private Option<T> RawTryGet<T>(Key<T> key)
      => Conf.TryGet<T>(Keys.InterpretFromDir(key.Id, Dir));

    public static IConf ChangeDir(IConf conf, IImmutableList<string> dir)
      => dir.Count == 0 ? conf : new SubDirConf(dir, conf);
  }
}