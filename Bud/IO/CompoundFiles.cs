using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bud.IO {
  public class CompoundFiles : IFiles {
    private readonly IFiles filesA;
    private readonly IFiles filesB;

    public CompoundFiles(IFiles filesA, IFiles filesB) {
      this.filesA = filesA;
      this.filesB = filesB;
    }

    public IEnumerable<string> Enumerate() => filesA.Enumerate().Concat(filesB.Enumerate());

    public IObservable<FilesUpdate> Watch()
      => filesA.Watch().Merge(filesB.Watch()).Select(update => new FilesUpdate(update.FileSystemEventArgs, this));
  }
}