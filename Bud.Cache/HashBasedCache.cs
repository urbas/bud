using System;
using System.IO;
using static System.IO.Path;

namespace Bud.Cache {
  /// <summary>
  ///   A simple file-system cache. Every entry in this cache is a directory. The name of the directory
  ///   is a hex string. This hex string is a unique identifier of the content. It is suggeste(cryptographic digest)
  /// </summary>
  public class HashBasedCache {
    public string Path { get; }

    /// <param name="path">the directory in which this cache will store all data.</param>
    public HashBasedCache(string path) {
      Path = path;
    }

    /// <param name="hash">a cryptographic hash that uniquely identifies the content.</param>
    /// <param name="onCacheMissContentProducer">
    ///   the callback method that should produce the content. The first parameter to this callback will be a directory where
    ///   the method should place all its content.
    /// </param>
    /// <returns>
    ///   path to the directory that contains the cached contents produced by <paramref name="onCacheMissContentProducer" />
    ///   for the given hash.
    /// </returns>
    public string Get(byte[] hash, Action<string> onCacheMissContentProducer) {
      var hashHex = ByteToHexString(hash);
      var cacheLocation = Combine(Path, hashHex);
      if (Directory.Exists(cacheLocation)) {
        return cacheLocation;
      }
      var temporaryDirectory = Combine(Path, $".tmp_{hashHex}_{Guid.NewGuid()}");
      Directory.CreateDirectory(temporaryDirectory);
      try {
        onCacheMissContentProducer(temporaryDirectory);
        try {
          Directory.Move(temporaryDirectory, cacheLocation);
        } catch (IOException e) {
          if (!Directory.Exists(cacheLocation)) {
            throw new Exception($"An unknown error occurred while creating the cache entry '{cacheLocation}'. Possible concurrent access.", e);
          }
        }
      } finally {
        if (Directory.Exists(temporaryDirectory)) {
          Directory.Delete(temporaryDirectory, true);
        }
      }
      return cacheLocation;
    }

    public static string ByteToHexString(byte[] bytes) {
      var c = new char[bytes.Length*2];
      int b;
      for (int i = 0; i < bytes.Length; i++) {
        b = bytes[i] >> 4;
        c[i*2] = (char) (55 + b + (((b - 10) >> 31) & -7));
        b = bytes[i] & 0xF;
        c[i*2 + 1] = (char) (55 + b + (((b - 10) >> 31) & -7));
      }
      return new string(c);
    }
  }
}