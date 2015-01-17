using System;
using System.IO;
using Bud.Test.Util;
using Bud.Plugins.BuildLoading;
using Bud.Commander;

namespace Bud.Test.Util {
  public class TemporaryDirBuildCommander : IBuildCommander {
    public readonly TemporaryDirectory TemporaryDirectory;
    private readonly IBuildCommander buildCommander;

    public TemporaryDirBuildCommander(TemporaryDirectory temporaryDirectory) {
      this.TemporaryDirectory = temporaryDirectory;
      this.buildCommander = BuildCommander.Load(TemporaryDirectory.Path);
    }

    public object Evaluate(string command) {
      return buildCommander.Evaluate(command);
    }

    public void Dispose() {
      Exception buildCommanderDisposeException = null;
      try {
        buildCommander.Dispose();
      } catch (Exception ex) {
        buildCommanderDisposeException = ex;
      }
      TemporaryDirectory.Dispose();

      if (buildCommanderDisposeException != null) {
        throw buildCommanderDisposeException;
      }
    }
  }
}

