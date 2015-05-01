using System.Collections.Generic;
using System.IO;
using Bud.Test.Util;
using NUnit.Framework;

namespace Bud.SystemTests.Examples {
  public class ExampleProjectsTest {
    private static readonly string ExamplesDir = Path.Combine(TestProjects.ThisProjectDir, "..", "Examples");

    [Test]
    public void Examples_MUST_produce_valid_settings() {
      foreach (var exampleDir in GetExampleDirs()) {
        using (var buildCommander = TestProjects.Load(exampleDir)) {
          buildCommander.EvaluateToJson("build");
          buildCommander.EvaluateToJson("test");
        }
      }
    }

    private IEnumerable<string> GetExampleDirs() {
      return Directory.EnumerateDirectories(ExamplesDir);
    }
  }
}