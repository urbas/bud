using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.IO;
using NUnit.Framework;
using static Bud.Build;

namespace Bud {
  public class BuildTest {
    private readonly Conf project = Project("bar", "Foo");

    [Test]
    public void Set_the_projectDir() => Assert.AreEqual("bar", ProjectDir[project]);

    [Test]
    public void Set_the_projectId() => Assert.AreEqual("Foo", ProjectId[project]);

    [Test]
    public void Set_the_directory_name_as_the_default_projectId()
      => Assert.AreEqual(Path.GetFileName("bar/moo"), ProjectId[Project("bar/moo")]);

    [Test]
    public void Sources_should_be_empty()
      => Assert.That(Sources[project].ToEnumerable().ToList(),
                     Is.EquivalentTo(ImmutableArray.Create(Enumerable.Empty<string>())));

    [Test]
    public void Multiple_source_directories() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A", "A.cs");
        var fileB = tempDir.CreateEmptyFile("B", "B.cs");
        var twoDirsProject = Project(tempDir.Path).Add(SourceDir("A"), SourceDir("B"));
        Assert.That(Sources[twoDirsProject].ToEnumerable().First(), Is.EquivalentTo(new[] {fileA, fileB}));
      }
    }
  }
}