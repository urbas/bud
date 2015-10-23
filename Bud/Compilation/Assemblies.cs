using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;

namespace Bud.Compilation {
  public class Assemblies : WatchedResources<AssemblyReference> {
    public static readonly Assemblies Empty = new Assemblies(Enumerable.Empty<AssemblyReference>());

    public Assemblies(IEnumerable<AssemblyReference> assemblies)
      : base(assemblies, Observable.Empty<AssemblyReference>()) {}

    public Assemblies(IEnumerable<AssemblyReference> assemblies, IObservable<AssemblyReference> watcher)
      : base(assemblies, watcher) {}

    public Assemblies(WatchedResources<AssemblyReference> watchedResources)
      : base(watchedResources) { }

    public Assemblies(IEnumerable<string> locations) : this(locations.Select(ToAssemblyReferenceFromFile)) {}

    public Assemblies(params string[] locations) : this((IEnumerable<string>)locations) {}

    public new Assemblies ExpandWith(IWatchedResources<AssemblyReference> other)
      => new Assemblies(base.ExpandWith(other));

    public Assemblies Add(AssemblyReference assemblyReference)
      => ExpandWith(new Assemblies(new[] { assemblyReference }));

    public new Assemblies WithFilter(Func<AssemblyReference, bool> filter)
      => new Assemblies(base.WithFilter(filter));

    public new IObservable<Assemblies> Watch()
      => base.Watch().Select(_ => this);

    private static AssemblyReference ToAssemblyReferenceFromFile(string file)
      => new AssemblyReference(file, MetadataReference.CreateFromFile(file));
  }
}