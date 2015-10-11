using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bud.IO;
using Microsoft.CodeAnalysis;

namespace Bud.Compilation {
  public static class ReferencesObservatory {
    public static IObservable<IEnumerable<Timestamped<Dependency>>> ObserveAssemblies(this IFilesObservatory filesObservatory, params string[] locations)
      => filesObservatory.ObserveFileList(locations).Select(_ => locations.Select(s => new Timestamped<Dependency>(new Dependency(s, MetadataReference.CreateFromFile(s)), FileTimestamps.Instance.GetTimestamp(s))));
  }
}