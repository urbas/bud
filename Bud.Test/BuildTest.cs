using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Bud.Compilation;
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
    public void Dependencies_should_be_empty()
      => Assert.That(Sources[project].ToEnumerable().ToList(),
                     Is.EquivalentTo(ImmutableArray.Create(Enumerable.Empty<ITimestamped>())));

    [Test]
    public void Multiple_source_directories() {
      using (var tempDir = new TemporaryDirectory()) {
        var fileA = tempDir.CreateEmptyFile("A", "A.cs");
        var fileB = tempDir.CreateEmptyFile("B", "B.cs");
        var twoDirsProject = Project(tempDir.Path, "foo").Add(SourceDir("A"), SourceDir("B"));
        Assert.That(Sources[twoDirsProject].ToEnumerable().First(),
                    Is.EquivalentTo(new[] {new Timestamped<string>(fileA, DateTimeOffset.Now), new Timestamped<string>(fileB, DateTimeOffset.Now) }));
      }
    }
  }
}