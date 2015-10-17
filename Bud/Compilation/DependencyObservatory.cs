using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;

namespace Bud.Compilation {
  public static class DependencyObservatory {
    public static IObservable<Assemblies> ObserveAssemblies(this IFilesObservatory filesObservatory, params string[] locations)
      => filesObservatory.ObserveFiles(locations)
                         .Select(_ => new Assemblies(locations.Select(ToTimestampedDependency)));

    private static Timestamped<Dependency> ToTimestampedDependency(string file)
      => new Timestamped<Dependency>(new Dependency(file, MetadataReference.CreateFromFile(file)), File.GetLastWriteTime(file));
  }
}