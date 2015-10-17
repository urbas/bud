using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bud.IO;

namespace Bud.Compilation {
  public struct Assemblies : IEnumerable<Hashed<AssemblyReference>>, IExpandable<Assemblies> {
    public static readonly Assemblies Empty = new Assemblies(Enumerable.Empty<Hashed<AssemblyReference>>());
    private readonly IEnumerable<Hashed<AssemblyReference>> files;

    public Assemblies(IEnumerable<Hashed<AssemblyReference>> files) {
      this.files = files;
    }

    public IEnumerator<Hashed<AssemblyReference>> GetEnumerator() => files.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public Assemblies ExpandWith(Assemblies other) => new Assemblies(files.Concat(other.files));
  }
}