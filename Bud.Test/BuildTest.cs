using System.IO;
using Bud.IO;
using Bud.Tasking.ApiV1;
using NUnit.Framework;
using static Bud.Build;

namespace Bud {
  public class BuildTest {
    private TemporaryDirectory tempDir;
    private Tasks cSharpProject;
    private Tasks bareProject;
    private Tasks twoSourceDirsProject;

    [SetUp]
    public void SetUp() {
      tempDir = new TemporaryDirectory();
      bareProject = Project(tempDir.Path, "Foo");
      cSharpProject = bareProject.ExtendWith(Sources(fileFilter: "*.cs"));
      twoSourceDirsProject = bareProject.ExtendWith(Sources("A")).ExtendWith(Sources("B"));
    }

    [TearDown]
    public void TearDown() => tempDir.Dispose();

    [Test]
    public async void Set_the_projectDir() {
      Assert.AreEqual(tempDir.Path, await bareProject.Get(ProjectDir));
    }

    [Test]
    public async void Set_the_projectId() {
      Assert.AreEqual("Foo", await bareProject.Get(ProjectId));
    }

    [Test]
    public async void Set_the_directory_name_as_the_default_projectId() {
      Assert.AreEqual(Path.GetFileName(tempDir.Path), await Project(tempDir.Path).Get(ProjectId));
    }

    [Test]
    public async void CSharp_sources_must_be_listed() {
      var sourceFile = tempDir.CreateFile("TestMainClass.cs");
      Assert.That(await cSharpProject.Get(SourceFiles), Contains.Item(sourceFile));
    }

    [Test]
    public async void CSharp_sources_in_nested_directories_must_be_listed() {
      var sourceFile = tempDir.CreateFile("Bud", "TestMainClass.cs");
      Assert.That(await cSharpProject.Get(SourceFiles), Contains.Item(sourceFile));
    }

    [Test]
    public async void Non_csharp_files_must_not_be_listed() {
      var textFile = tempDir.CreateFile("Bud", "TextFile.txt");
      Assert.That(await cSharpProject.Get(SourceFiles), Is.Not.Contains(textFile));
    }

    [Test]
    public async void Multiple_source_directories() {
      var fileA = tempDir.CreateFile("A", "A.cs");
      var fileB = tempDir.CreateFile("B", "B.cs");
      Assert.That(await twoSourceDirsProject.Get(SourceFiles), Is.EquivalentTo(new[] {fileA, fileB}));
    }
  }
}