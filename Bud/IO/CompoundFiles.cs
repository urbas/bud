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

    public IObservable<IFiles> AsObservable()
      => filesA.AsObservable().CombineLatest(filesB.AsObservable(), (files, files1) => this);

    public IEnumerator<string> GetEnumerator() => filesA.Concat(filesB).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}