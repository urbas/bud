using System.IO;
using Bud.IO;
using Bud.Resources;
using Bud.Tasking;
using NUnit.Framework;
using static Bud.Build;

namespace Bud {
  public class BuildTest {
    private TemporaryDirectory tempDir;
    private Tasks fooBarProject;

    [SetUp]
    public void SetUp() {
      tempDir = new TemporaryDirectory();
      fooBarProject = Project(tempDir.Path, "Foo").ExtendWith(SourceFiles(fileFilter: "*.cs"));
    }

    [TearDown]
    public void TearDown() => tempDir.Dispose();

    [Test]
    public async void Set_the_projectDir() {
      Assert.AreEqual(tempDir.Path, await ProjectDir[fooBarProject]);
    }

    [Test]
    public async void Set_the_projectId() {
      Assert.AreEqual("Foo", await ProjectId[fooBarProject]);
    }

    [Test]
    public async void Set_the_directory_name_as_the_default_projectId() {
      Assert.AreEqual(Path.GetFileName(tempDir.Path), await ProjectId[Project(tempDir.Path)]);
    }

    [Test]
    public async void CSharp_sources_must_be_listed() {
      var sourceFile = Path.Combine(tempDir.Path, "TestMainClass.cs");
      await EmbeddedResources.CopyTo(GetType().Assembly, "TestMainClass.cs", sourceFile);
      Assert.That(await Sources[fooBarProject], Contains.Item(sourceFile));
    }

    [Test]
    public async void CSharp_sources_in_nested_directories_must_be_listed() {
      var sourceFile = Path.Combine(tempDir.Path, "Bud", "TestMainClass.cs");
      await EmbeddedResources.CopyTo(GetType().Assembly, "TestMainClass.cs", sourceFile);
      Assert.That(await Sources[fooBarProject], Contains.Item(sourceFile));
    }

    [Test]
    public async void Non_csharp_files_must_not_be_listed() {
      var textFile = Path.Combine(tempDir.Path, "Bud", "TextFile.txt");
      await EmbeddedResources.CopyTo(GetType().Assembly, "TestMainClass.cs", textFile);
      Assert.That(await Sources[fooBarProject], Is.Not.Contains(textFile));
    }
  }
}