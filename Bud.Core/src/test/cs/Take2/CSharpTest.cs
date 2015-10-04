using System.IO;
using Bud.IO;
using Bud.Tasking;
using NUnit.Framework;

namespace Bud.Take2 {
  public class CSharpTest {
    private TemporaryDirectory tempDir;
    private Tasks fooBarProject;

    [SetUp]
    public void SetUp() {
      tempDir = new TemporaryDirectory();
      fooBarProject = Build.Project(tempDir.Path, "Foo.Bar").ExtendWith(CSharp.Project());
    }

    [TearDown]
    public void TearDown() => tempDir.Dispose();

    [Test]
    public async void output_assembly_must_be_placed_in_the_target_folder() {
      Assert.AreEqual(Path.Combine(tempDir.Path, "target", "Foo.Bar.exe"),
                      await CSharp.OutputAssembly[fooBarProject]);
    }
  }
}