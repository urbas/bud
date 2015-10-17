using System;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.Compilation {
  public static class AssembliesStream {
    public static IObservable<Assemblies> AddAssemblies(this IObservable<Assemblies> stream, IObservable<Assemblies> otherStream)
      => stream.CombineLatest(otherStream, (collection, otherCollection) => new Assemblies(Enumerable.Concat(collection, otherCollection)));
  }
}