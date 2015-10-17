using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bud.IO;

namespace Bud.Compilation {
  public struct Assemblies : IEnumerable<Hashed<Dependency>>, IExpandable<Assemblies> {
    public static readonly Assemblies Empty = new Assemblies(Enumerable.Empty<Hashed<Dependency>>());
    private readonly IEnumerable<Hashed<Dependency>> files;

    public Assemblies(IEnumerable<Hashed<Dependency>> files) {
      this.files = files;
    }

    public IEnumerator<Hashed<Dependency>> GetEnumerator() => files.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public Assemblies ExpandWith(Assemblies other) => new Assemblies(files.Concat(other.files));
  }
}