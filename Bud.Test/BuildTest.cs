using System.Collections.Immutable;
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
    public void Sources_should_be_initially_empty()
      => Assert.IsEmpty(Sources[project]);

    [Test]
    public void Dependencies_should_be_initially_empty()
      => Assert.IsEmpty(Dependencies[project]);

    [Test]
    public void Multiple_source_directories() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A", "A.cs");
        var fileB = tempDir.CreateEmptyFile("B", "B.cs");
        var twoDirsProject = Project(tempDir.Path, "foo").Add(SourceDir("A"), SourceDir("B"));
        Assert.That(Sources[twoDirsProject],
                    Is.EquivalentTo(new[] {Files.ToTimeHashedFile(fileA), Files.ToTimeHashedFile(fileB)}));
      }
    }
  }
}