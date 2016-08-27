using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Bud.Building {
  public static class Hasher {
    /// <summary>
    ///   Calculates the hash of multiple files as if they formed one continuous stream of data.
    /// </summary>
    /// <param name="files">the files whose contents will be digested into a hash.</param>
    /// <param name="salt">
    ///   a bit of randomness that will be digested together with the files.
    ///   These bytes are digested as the first block (before any of the files).
    ///   This parameter is optional. An empty array is used by default.
    /// </param>
    /// <param name="hashAlgorithm">
    ///   the hashing alrogithm with which to generate the hash.
    ///   This parameter is optiona. <see cref="SHA256" /> is used by default.
    /// </param>
    /// <returns>the hash of the digested contents of the files.</returns>
    public static byte[] HashFiles(IEnumerable<string> files, byte[] salt = null, HashAlgorithm hashAlgorithm = null) {
      var digest = Init(salt ?? Array.Empty<byte>(), hashAlgorithm ?? SHA256.Create());
      var buffer = CreateBuffer();
      foreach (var file in files) {
        AddFile(file, buffer, digest);
      }
      return Finish(digest, buffer);
    }

    private static byte[] Finish(HashAlgorithm digest, byte[] buffer) {
      digest.TransformFinalBlock(buffer, 0, 0);
      return digest.Hash;
    }

    private static HashAlgorithm Init(byte[] salt, HashAlgorithm hashingAlgorithm) {
      hashingAlgorithm.Initialize();
      hashingAlgorithm.TransformBlock(salt, 0, salt.Length, salt, 0);
      return hashingAlgorithm;
    }

    private static void AddFile(string file, byte[] buffer, ICryptoTransform digest) {
      using (var fileStream = File.OpenRead(file)) {
        int readBytes;
        do {
          readBytes = fileStream.Read(buffer, 0, buffer.Length);
          digest.TransformBlock(buffer, 0, readBytes, buffer, 0);
        } while (readBytes == buffer.Length);
      }
    }

    private static byte[] CreateBuffer() => new byte[1 << 15];
  }
}