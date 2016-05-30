using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Bud.Building {
  public static class Md5Hasher {
    public static byte[] Digest(IEnumerable<string> files, byte[] salt) {
      var digest = Init(salt);
      var buffer = CreateBuffer();
      foreach (var file in files) {
        AddFile(file, buffer, digest);
      }
      return Finish(digest, buffer);
    }

    public static byte[] Digest(string file, byte[] salt) {
      var digest = Init(salt);
      var buffer = CreateBuffer();
      AddFile(file, buffer, digest);
      return Finish(digest, buffer);
    }

    private static byte[] Finish(HashAlgorithm digest, byte[] buffer) {
      digest.TransformFinalBlock(buffer, 0, 0);
      return digest.Hash;
    }

    private static MD5 Init(byte[] salt) {
      var digest = MD5.Create();
      digest.Initialize();
      digest.TransformBlock(salt, 0, salt.Length, salt, 0);
      return digest;
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