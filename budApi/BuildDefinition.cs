using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud {
  public interface BuildDefinition {
    Settings GetSettings(string baseDir);
  }
}

