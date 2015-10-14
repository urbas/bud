using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using NUnit.Framework;
using static Bud.Build;

namespace Bud {
  public class BuildTest {
    private TemporaryDirectory tempDir;
    private Conf cSharpProject;
    private Conf bareProject;
    private Conf twoSourceDirsProject;

    [SetUp]
    public void SetUp() {
      tempDir = new TemporaryDirectory();
      bareProject = Project(tempDir.Path, "Foo");
      cSharpProject = bareProject.Add(SourceDir(fileFilter: "*.cs"));
      twoSourceDirsProject = bareProject.Add(SourceDir("A"), SourceDir("B"));
    }

    [TearDown]
    public void TearDown() => tempDir.Dispose();

    [Test]
    public void Set_the_projectDir() {
      Assert.AreEqual(tempDir.Path, bareProject.Get(ProjectDir));
    }

    [Test]
    public void Set_the_projectId() {
      Assert.AreEqual("Foo", bareProject.Get(ProjectId));
    }

    [Test]
    public void Set_the_directory_name_as_the_default_projectId() {
      Assert.AreEqual(Path.GetFileName(tempDir.Path), Project(tempDir.Path).Get(ProjectId));
    }

    [Test]
    public void CSharp_sources_must_be_listed() {
      var sourceFile = tempDir.CreateEmptyFile("TestMainClass.cs");
      Assert.That(Sources[cSharpProject].ToEnumerable().First(), Contains.Item(sourceFile));
    }

    [Test]
    public void CSharp_sources_in_nested_directories_must_be_listed() {
      var sourceFile = tempDir.CreateEmptyFile("Bud", "TestMainClass.cs");
      Assert.That(Sources[cSharpProject].ToEnumerable().First(), Contains.Item(sourceFile));
    }

    [Test]
    public void Non_csharp_files_must_not_be_listed() {
      var textFile = tempDir.CreateEmptyFile("Bud", "TextFile.txt");
      Assert.That(Sources[cSharpProject].ToEnumerable().First(), Is.Not.Contains(textFile));
    }

    [Test]
    public void Multiple_source_directories() {
      var fileA = tempDir.CreateEmptyFile("A", "A.cs");
      var fileB = tempDir.CreateEmptyFile("B", "B.cs");
      Assert.That(Sources[twoSourceDirsProject].ToEnumerable().First(), Is.EquivalentTo(new[] {fileA, fileB}));
    }
  }
}