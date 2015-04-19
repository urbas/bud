using System;
using Bud.Commander;

namespace Bud.Test.Util {
  public class TemporaryDirBuildCommander : IBuildCommander {
    public readonly TemporaryDirectory TemporaryDirectory;
    private readonly IBuildCommander BuildCommander;

    public TemporaryDirBuildCommander(TemporaryDirectory temporaryDirectory) {
      TemporaryDirectory = temporaryDirectory;
      try {
        BuildCommander = Commander.BuildCommander.LoadProjectLevelCommander(TemporaryDirectory.Path);
      } catch (Exception) {
        TemporaryDirectory.Dispose();
        throw;
      }
    }

    public string EvaluateToJson(string command) => BuildCommander.EvaluateToJson(command);

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