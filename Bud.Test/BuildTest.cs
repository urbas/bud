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
      fooBarProject = Project(tempDir.Path, "Foo.Bar").ExtendWith(SourceFiles("*.cs"));
    }

    [TearDown]
    public void TearDown() => tempDir.Dispose();

    [Test]
    public async void should_set_the_projectDir() {
      Assert.AreEqual("foo", await ProjectDir[Project("foo", "Foo")]);
    }

    [Test]
    public async void should_set_the_projectId() {
      Assert.AreEqual("Foo", await ProjectId[Project("foo", "Foo")]);
    }

    [Test]
    public async void CSharp_sources_must_be_listed() {
      var sourceFile = Path.Combine(tempDir.Path, "src", "TestMainClass.cs");
      await EmbeddedResources.CopyTo(GetType().Assembly, "TestMainClass.cs", sourceFile);
      Assert.That(await Sources[fooBarProject], Contains.Item(sourceFile));
    }

    [Test]
    public async void CSharp_sources_in_nested_directories_must_be_listed() {
      var sourceFile = Path.Combine(tempDir.Path, "src", "Bud", "TestMainClass.cs");
      await EmbeddedResources.CopyTo(GetType().Assembly, "TestMainClass.cs", sourceFile);
      Assert.That(await Sources[fooBarProject], Contains.Item(sourceFile));
    }

    [Test]
    public async void Non_csharp_files_must_not_be_listed() {
      var textFile = Path.Combine(tempDir.Path, "src", "Bud", "TextFile.txt");
      await EmbeddedResources.CopyTo(GetType().Assembly, "TestMainClass.cs", textFile);
      Assert.That(await Sources[fooBarProject], Is.Not.Contains(textFile));
    }
  }
}