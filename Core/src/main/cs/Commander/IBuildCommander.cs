using System;

namespace Bud.Commander {
  public interface IBuildCommander : IDisposable {
    string EvaluateToJson(string command);
  }
}