using System;

namespace Bud {
  public interface BudPlugin {
    Settings ApplyTo(Settings settings, Scope scope);
  }
}

