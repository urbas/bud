using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;

namespace Bud.Compilation {
  public class Assemblies : WatchedResources<Hashed<AssemblyReference>> {
    public static readonly Assemblies Empty = new Assemblies(Enumerable.Empty<Hashed<AssemblyReference>>());

    public Assemblies(IEnumerable<Hashed<AssemblyReference>> files)
      : base(() => files, Observable.Empty<object>) {}

    public Assemblies(WatchedResources<Hashed<AssemblyReference>> watchedResources)
      : base(watchedResources) { }

    public Assemblies ExpandWith(Assemblies other)
      => new Assemblies(base.ExpandWith(other));

    public new Assemblies WithFilter(Func<Hashed<AssemblyReference>, bool> filter)
      => new Assemblies(base.WithFilter(filter));

    public new IObservable<Assemblies> Watch()
      => base.Watch().Select(_ => this);
  }
}