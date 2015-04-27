using System;

namespace Bud.Commander {
  public interface IBuildCommander : IDisposable {
    string EvaluateToJson(string command);
    string EvaluateMacroToJson(string macroName, params string[] commandLineParameters);
  }
}