using System;
using System.Collections.Immutable;
using System.IO;
using Bud.TempDir;
using Moq;
using NUnit.Framework;

namespace Bud.Building {
  public class TimestampBasedBuildingTest {
    private TemporaryDirectory dir;
    private Mock<IOutputGenerator> outputGenerator;

    [SetUp]
    public void SetUp() {
      outputGenerator = new Mock<IOutputGenerator>(MockBehavior.Strict);
      dir = new TemporaryDirectory();
    }

    [TearDown]
    public void TearDown() => dir.Dispose();

    [Test]
    public void Build_creates_the_output_file() {
      var output = dir.CreatePath("a.out");
      outputGenerator.Setup(self => self.Generate(output, ImmutableList<string>.Empty));
      TimestampBasedBuilding.Build(outputGenerator.Object, output, ImmutableList<string>.Empty);
      outputGenerator.VerifyAll();
    }

    [Test]
    public void Build_does_not_invoke_the_worker_when_output_already_exists() {
      var output = dir.CreateEmptyFile("a.out");
      TimestampBasedBuilding.Build(outputGenerator.Object, output, ImmutableList<string>.Empty);
    }

    [Test]
    public void Build_invokes_the_worker_when_the_output_is_stale() {
      var output = dir.CreateEmptyFile("a.out");
      var fileA = dir.CreateFile("foo", "a");
      File.SetLastWriteTime(output, File.GetLastWriteTime(fileA) - TimeSpan.FromSeconds(1));
      var input = ImmutableList.Create(fileA);
      outputGenerator.Setup(self => self.Generate(output, input));

      TimestampBasedBuilding.Build(outputGenerator.Object, output, input);

      outputGenerator.VerifyAll();
    }

    [Test]
    public void Build_does_not_invoke_the_worker_when_the_output_is_up_to_date() {
      var fileA = dir.CreateFile("foo", "a");
      var output = dir.CreateEmptyFile("a.out");
      File.SetLastWriteTime(fileA, File.GetLastWriteTime(output) - TimeSpan.FromSeconds(1));
      var inputFiles = ImmutableList.Create(fileA);
      TimestampBasedBuilding.Build(outputGenerator.Object, output, inputFiles);
    }
  }
}