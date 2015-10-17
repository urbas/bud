using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bud.IO {
  public struct Files : IEnumerable<Hashed<string>>, IExpandable<Files> {
    public static readonly Files Empty = new Files(Enumerable.Empty<Hashed<string>>());
    private readonly IEnumerable<Hashed<string>> files;

    public Files(IEnumerable<Hashed<string>> files) {
      this.files = files;
    }

    public Files(IEnumerable<string> files) {
      this.files = files.Select(ToTimeHashedFile);
    }

    public IEnumerator<Hashed<string>> GetEnumerator() => files.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public Files ExpandWith(Files other)
      => new Files(files.Concat(other.files));

    public static Hashed<string> ToTimeHashedFile(string path)
      => new Hashed<string>(path, GetTimeHash(path));

    public static long GetTimeHash(string file)
      => File.GetLastWriteTime(file).ToFileTime();

    public static long GetTimeHash()
      => DateTime.Now.ToFileTime();

    public static bool TimeHashEquals<T>(Hashed<T> timeHashedFile, string file)
      => timeHashedFile.Hash != GetTimeHash(file);
  }
}