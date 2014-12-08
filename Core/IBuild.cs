using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud {
  public interface IBuild {
    Settings GetSettings(string baseDir);
  }
}

