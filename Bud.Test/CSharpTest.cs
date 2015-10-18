using Bud.IO;
using NUnit.Framework;
using static Bud.Build;

namespace Bud {
  public class CSharpTest {
    private TemporaryDirectory tempDir;
    private Conf cSharpProject;
    private Conf bareProject;

    [SetUp]
    public void SetUp() {
      tempDir = new TemporaryDirectory();
      bareProject = Project(tempDir.Path, "Foo");
      cSharpProject = bareProject.Add(SourceDir(fileFilter: "*.cs"));
    }

    [TearDown]
    public void TearDown() => tempDir.Dispose();

    [Test]
    public void CSharp_sources_must_be_listed() {
      var sourceFile = tempDir.CreateEmptyFile("TestMainClass.cs");
      Assert.That(Sources[cSharpProject],
                  Contains.Item(sourceFile));
    }

    [Test]
    public void CSharp_sources_in_nested_directories_must_be_listed() {
      var sourceFile = tempDir.CreateEmptyFile("Bud", "TestMainClass.cs");
      Assert.That(Sources[cSharpProject],
                  Contains.Item(sourceFile));
    }

    [Test]
    public void Non_csharp_files_must_not_be_listed() {
      var textFile = tempDir.CreateEmptyFile("Bud", "TextFile.txt");
      Assert.That(Sources[cSharpProject],
                  Is.Not.Contains(textFile));
    }
  }
}