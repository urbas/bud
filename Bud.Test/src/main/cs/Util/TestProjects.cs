using System;
using System.IO;

namespace Bud.Test.Util {
  public static class TestProjects {
    public static readonly string TestAssemblyDir = Path.GetDirectoryName(typeof(TestProjects).Assembly.Location);
    public static readonly string TestProjectsBaseDir = Path.Combine(TestAssemblyDir, "..", "..", "..", "..", "..", "src", "test", "budProjects");

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
      return Path.Combine(TestProjectsBaseDir, projectDirName);
    }

    public static TemporaryDirBuildCommander LoadBuildCommander(object systemTestClassInstance) {
      var testProjectDirName = systemTestClassInstance.GetType().Name;
      return LoadBuildCommander(testProjectDirName);
    }
  }
}