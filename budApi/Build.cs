using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bud {
  public interface Build {
    ImmutableList<Setting> Settings();
  }
}

