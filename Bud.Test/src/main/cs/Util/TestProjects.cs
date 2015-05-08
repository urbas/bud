using System.IO;
using Bud.IO;

namespace Bud.Test.Util {
  public static class TestProjects {
    public static readonly string TestAssemblyDir = Path.GetDirectoryName(typeof(TestProjects).Assembly.Location);
    public static readonly string ThisProjectDir = Path.Combine(TestAssemblyDir, "..", "..", "..", "..", "..");
    public static readonly string TestProjectsBaseDir = Path.Combine(ThisProjectDir, "src", "test", "budProjects");

    public static TemporaryDirBuildCommander Load() {
      var emptyProject = new TemporaryDirectory();
      return new TemporaryDirBuildCommander(emptyProject);
    }

    public static TemporaryDirBuildCommander Load(object systemTestClassInstance) {
      var testProjectName = systemTestClassInstance.GetType().Name;
      var temporaryCopy = new TemporaryDirectory(GetPathOfProject(testProjectName));
      return new TemporaryDirBuildCommander(temporaryCopy);
    }

    public static TemporaryDirBuildCommander Load(string projectDir) {
      return new TemporaryDirBuildCommander(new TemporaryDirectory(projectDir));
    }

    private static string GetPathOfProject(string projectDirName) {
      return Path.Combine(TestProjectsBaseDir, projectDirName);
    }
  }
}