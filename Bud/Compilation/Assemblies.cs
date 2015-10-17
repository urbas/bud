using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bud.IO;

namespace Bud.Compilation {
  public struct Assemblies : IEnumerable<Timestamped<Dependency>> {
    public static readonly Assemblies Empty = new Assemblies(Enumerable.Empty<Timestamped<Dependency>>());
    private readonly IEnumerable<Timestamped<Dependency>> files;

    public Assemblies(IEnumerable<Timestamped<Dependency>> files) {
      this.files = files;
    }

    public IEnumerator<Timestamped<Dependency>> GetEnumerator() => files.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}