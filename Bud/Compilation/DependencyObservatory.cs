using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bud.IO;

namespace Bud.Compilation {
  public static class DependencyObservatory {
    public static IObservable<IEnumerable<Timestamped<Dependency>>> ObserveAssemblies(this IFilesObservatory filesObservatory, params string[] locations)
      => filesObservatory.ObserveFileList(locations)
                         .Select(_ => locations.Select(ToTimestampedDependency));

    private static Timestamped<Dependency> ToTimestampedDependency(string file)
      => new Timestamped<Dependency>(Dependency.CreateFromFile(file),
                                     File.GetLastWriteTime(file));
  }
}