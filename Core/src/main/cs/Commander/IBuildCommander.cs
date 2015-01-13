using System;

namespace Bud.Commander {
  public interface IBuildCommander : IDisposable {
    object Evaluate(string command);
  }
}