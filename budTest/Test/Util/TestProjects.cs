using System;
using System.IO;
using Bud.Test.Util;

namespace Bud {
  public static class TestProjects {
    public static TemporaryDirectoryCopy TemporaryCopy(string projectDirName) {
      return new TemporaryDirectoryCopy(GetPathOfProject(projectDirName));
    }

    public static string GetPathOfProject(string projectDirName) {
      return Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "testProjects", projectDirName);
    }
  }
}

