using System.Collections.Immutable;
using Bud.Cache;

namespace Bud.Building {
  public class CachingHashBasedBuilder {
    /// <param name="dirBuilder">
    ///   this generator will be invoked if the content has not been found in the cache.
    /// </param>
    /// <param name="cache">
    ///   the cache where to look for already generated content and where to place newly
    ///   generated content.
    /// </param>
    /// <param name="input">
    ///   a list of files used as input by <paramref name="dirBuilder" />.
    ///   If the build has been previously invoked with exactly the same files, then
    ///   the cache should contain the output and <paramref name="dirBuilder" />
    ///   will not be invoked.
    /// </param>
    /// <param name="salt">
    ///   this salt is used when calculating the hash of <paramref name="input" />.
    ///   It can be used to invalidate caches if the version of the builder changes.
    /// </param>
    /// <returns>
    ///   the directory that contains the output produced by <paramref name="dirBuilder" />.
    /// </returns>
    public static string Build(DirFromFilesBuilder dirBuilder,
                               HashDirCache cache,
                               IImmutableList<string> input,
                               byte[] salt)
      => cache.Get(Md5Hasher.Digest(input, salt),
                   cacheDir => dirBuilder(input, cacheDir));
  }
}