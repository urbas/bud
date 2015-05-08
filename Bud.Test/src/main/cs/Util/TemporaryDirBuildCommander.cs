using System;
using Bud.Commander;
using Bud.IO;

namespace Bud.Test.Util {
  public class TemporaryDirBuildCommander : IBuildCommander {
    public readonly TemporaryDirectory TemporaryDirectory;
    private readonly IBuildCommander BuildCommander;

    public TemporaryDirBuildCommander(TemporaryDirectory temporaryDirectory) {
      TemporaryDirectory = temporaryDirectory;
      try {
        BuildCommander = Commander.BuildCommander.LoadProjectLevelCommander(TemporaryDirectory.Path, false);
      } catch (Exception) {
        TemporaryDirectory.Dispose();
        throw;
      }
    }

    public string EvaluateToJson(string command) => BuildCommander.EvaluateToJson(command);

    public string EvaluateMacroToJson(string macroName, params string[] commandLineParameters) => BuildCommander.EvaluateMacroToJson(macroName, commandLineParameters);

    public void Dispose() {
      Exception buildCommanderDisposeException = null;
      try {
        BuildCommander.Dispose();
      } catch (Exception ex) {
        buildCommanderDisposeException = ex;
      }
      TemporaryDirectory.Dispose();

      if (buildCommanderDisposeException != null) {
        throw buildCommanderDisposeException;
      }
    }

    public override string ToString() => TemporaryDirectory.ToString();
  }
}