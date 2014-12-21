using System;
using System.IO;
using Bud.Test.Util;

namespace Bud.Test.Util {
  public static class TestProjects {
    public static TemporaryDirBuildCommander LoadBuildCommander(string projectDirName) {
      return new TemporaryDirBuildCommander(TemporaryCopy(projectDirName));
    }

    public static TemporaryDirBuildCommander LoadBuildCommander() {
      return new TemporaryDirBuildCommander(EmptyProject());
    }

    public static TemporaryDirectory TemporaryCopy(string projectDirName) {
      return new TemporaryDirectory(GetPathOfProject(projectDirName));
    }

    public static TemporaryDirectory EmptyProject() {
      return new TemporaryDirectory();
    }

    public static string GetPathOfProject(string projectDirName) {
      return Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "testProjects", projectDirName);
    }
  }
}
