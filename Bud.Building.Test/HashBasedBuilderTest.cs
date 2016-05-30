using System.Collections.Immutable;
using System.IO;
using Moq;
using NUnit.Framework;

namespace Bud.Building {
  public class HashBasedBuilderTest {
    private TmpDir dir;
    private Mock<DigestGenerator> outputGenerator;

    [SetUp]
    public void SetUp() {
      outputGenerator = new Mock<DigestGenerator> {CallBase = true};
      dir = new TmpDir();
    }

    [TearDown]
    public void TearDown() => dir.Dispose();

    [Test]
    public void Build_creates_the_output_file() {
      var output = dir.CreatePath("a.out");
      HashBasedBuilder.Build(outputGenerator.Object, output, ImmutableList<string>.Empty);
      outputGenerator.Verify(self => self.Generate(output, ImmutableList<string>.Empty),
                             Times.Once);
    }

    [Test]
    public void Build_not_invoked_second_time() {
      var output = dir.CreatePath("a.out");
      HashBasedBuilder.Build(outputGenerator.Object, output, ImmutableList<string>.Empty);
      HashBasedBuilder.Build(outputGenerator.Object, output, ImmutableList<string>.Empty);
      outputGenerator.Verify(self => self.Generate(output, ImmutableList<string>.Empty), Times.Once);
    }

    [Test]
    public void Build_invoked_when_file_added() {
      var output = dir.CreatePath("a.out");
      var noFiles = ImmutableList<string>.Empty;
      HashBasedBuilder.Build(outputGenerator.Object, output, noFiles);
      var singleFile = ImmutableList.Create(dir.CreateFile("foo", "foo"));
      HashBasedBuilder.Build(outputGenerator.Object, output, singleFile);
      outputGenerator.Verify(self => self.Generate(output, singleFile), Times.Once);
    }

    [Test]
    public void Build_invoked_when_file_changed() {
      var output = dir.CreatePath("a.out");
      var fileFoo = dir.CreateFile("foo", "foo");
      var singleFile = ImmutableList.Create(fileFoo);
      HashBasedBuilder.Build(outputGenerator.Object, output, singleFile);
      File.WriteAllText(fileFoo, "foobar");
      HashBasedBuilder.Build(outputGenerator.Object, output, singleFile);
      outputGenerator.Verify(self => self.Generate(output, singleFile), Times.Exactly(2));
    }

    [Test]
    public void Build_invoked_when_salt_changes() {
      var output = dir.CreatePath("a.out");
      var inputHashFile = dir.CreatePath("a.out.input_hash");
      var fileFoo = dir.CreateFile("foo", "foo");
      var singleFile = ImmutableList.Create(fileFoo);
      HashBasedBuilder.Build(outputGenerator.Object, output, singleFile, inputHashFile, new byte[] {0x00});
      HashBasedBuilder.Build(outputGenerator.Object, output, singleFile, inputHashFile, new byte[] {0x01});
      outputGenerator.Verify(self => self.Generate(output, singleFile), Times.Exactly(2));
    }
  }
}