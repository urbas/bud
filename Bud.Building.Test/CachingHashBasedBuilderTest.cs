using System.Collections.Immutable;
using System.IO;
using Bud.Cache;
using Bud.TempDir;
using Moq;
using NUnit.Framework;

namespace Bud.Building {
  public class CachingHashBasedBuilderTest {
    private static readonly byte[] Salt = {0x13};
    private Mock<IDirContentGenerator> generatorMock;
    private TemporaryDirectory dir;
    private static readonly ImmutableList<string> EmptyInput = ImmutableList<string>.Empty;

    [SetUp]
    public void SetUp() {
      generatorMock = new Mock<IDirContentGenerator>();
      dir = new TemporaryDirectory();
    }

    [TearDown]
    public void TearDown() => dir.Dispose();

    [Test]
    public void Build_invokes_the_generator_when_cache_does_not_contain_the_output() {
      CachingHashBasedBuilder.Build(generatorMock.Object, new HashBasedCache(dir.Path), EmptyInput, Salt);
      generatorMock.Verify(self => self.Generate(It.IsAny<string>(), EmptyInput), Times.Once);
    }

    [Test]
    public void Build_invokes_the_generator_only_once() {
      CachingHashBasedBuilder.Build(generatorMock.Object, new HashBasedCache(dir.Path), EmptyInput, Salt);
      CachingHashBasedBuilder.Build(generatorMock.Object, new HashBasedCache(dir.Path), EmptyInput, Salt);
      generatorMock.Verify(self => self.Generate(It.IsAny<string>(), EmptyInput), Times.Once);
    }

    [Test]
    public void Build_reinvokes_the_generator_when_input_changes() {
      var someInput = ImmutableList.Create(dir.CreateFile("42", "a"));
      CachingHashBasedBuilder.Build(generatorMock.Object, new HashBasedCache(dir.Path), EmptyInput, Salt);
      CachingHashBasedBuilder.Build(generatorMock.Object, new HashBasedCache(dir.Path), someInput, Salt);
      generatorMock.Verify(self => self.Generate(It.IsAny<string>(), It.IsAny<IImmutableList<string>>()), Times.Exactly(2));
    }

    [Test]
    public void Build_returns_the_directory_in_which_the_generator_produced_output() {
      generatorMock.Setup(self => self.Generate(It.IsAny<string>(), EmptyInput))
                   .Callback<string, IImmutableList<string>>((outputDir, input) => CreateFooFile(outputDir));
      var cacheDir = CachingHashBasedBuilder.Build(generatorMock.Object,
                                                  new HashBasedCache(dir.Path), EmptyInput, Salt);
      FileAssert.AreEqual(CreateFooFile(dir.Path),
                          Path.Combine(cacheDir, "foo"));
    }

    private static string CreateFooFile(string cacheDir) {
      var fooFile = Path.Combine(cacheDir, "foo");
      File.WriteAllText(fooFile, "42");
      return fooFile;
    }
  }
}