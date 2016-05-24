using System.Collections.Immutable;
using Bud.Cache;

namespace Bud.Building {
  public class CachedHashBasedBuilder {
    public static string Build(IDirectoryContentGenerator directoryContentGenerator, HashBasedCache cache, IImmutableList<string> input, byte[] salt)
      => cache.Get(Hasher.Md5(input, salt),
                   cacheDir => directoryContentGenerator.Generate(cacheDir, input));
  }
}