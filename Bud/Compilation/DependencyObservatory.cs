using System;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;

namespace Bud.Compilation {
  public static class DependencyObservatory {
    public static IObservable<Assemblies> ObserveAssemblies(this IFilesObservatory filesObservatory, params string[] locations)
      => filesObservatory.ObserveFiles(locations)
                         .Select(_ => new Assemblies(locations.Select(ToTimestampedDependency)));

    private static Hashed<Dependency> ToTimestampedDependency(string file)
      => new Hashed<Dependency>(new Dependency(file, MetadataReference.CreateFromFile(file)), Files.GetTimeHash(file));
  }
}