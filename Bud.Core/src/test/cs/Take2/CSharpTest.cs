using System.IO;
using Bud.IO;
using Bud.Tasking;
using NUnit.Framework;
using static Bud.Take2.Build;
using static Bud.Take2.CSharp;

namespace Bud.Take2 {
  public class CSharpTest {
    private TemporaryDirectory tempDir;
    private Tasks fooBarProject;

    [SetUp]
    public void SetUp() {
      tempDir = new TemporaryDirectory();
      fooBarProject = Project(tempDir.Path, "Foo.Bar").ExtendWith(CSharpCompiler());
    }

    [TearDown]
    public void TearDown() => tempDir.Dispose();

    [Test]
    public async void output_assembly_must_be_placed_in_the_target_folder() {
      Assert.AreEqual(Path.Combine(tempDir.Path, "target", "Foo.Bar.exe"),
                      await OutputAssembly[fooBarProject]);
    }
  }
}