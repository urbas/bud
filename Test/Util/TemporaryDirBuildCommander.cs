using System;
using System.IO;
using Bud.Test.Util;
using Bud.Plugins.BuildLoading;
using Bud.Commander;

namespace Bud.Test.Util {
  public class TemporaryDirBuildCommander : IBuildCommander {
    public readonly TemporaryDirectory TemporaryDirectory;
    public readonly IBuildCommander BuildCommander;

    public TemporaryDirBuildCommander(TemporaryDirectory temporaryDirectory) {
      this.TemporaryDirectory = temporaryDirectory;
      this.BuildCommander = BuildLoading.Load(TemporaryDirectory.Path);
    }

    public string Evaluate(string command) {
      return BuildCommander.Evaluate(command);
    }

    public void Dispose() {
      Exception tempDirDisposeException = null;
      try {
        TemporaryDirectory.Dispose();
      } catch (Exception ex) {
        tempDirDisposeException = ex;
      }
      BuildCommander.Dispose();

      if (tempDirDisposeException != null) {
        throw tempDirDisposeException;
      }
    }
  }

}

