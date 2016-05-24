using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Bud.Building {
  public static class Hasher {
    public static byte[] Md5(IEnumerable<string> files, byte[] salt) {
      var digest = MD5.Create();
      digest.Initialize();
      digest.TransformBlock(salt, 0, salt.Length, salt, 0);
      var buffer = new byte[1 << 15];
      foreach (var file in files) {
        using (var fileStream = File.OpenRead(file)) {
          int readBytes;
          do {
            readBytes = fileStream.Read(buffer, 0, buffer.Length);
            digest.TransformBlock(buffer, 0, readBytes, buffer, 0);
          } while (readBytes == buffer.Length);
        }
      }
      digest.TransformFinalBlock(buffer, 0, 0);
      return digest.Hash;
    }
  }
}