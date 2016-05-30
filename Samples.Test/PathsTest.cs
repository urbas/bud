using NUnit.Framework;

namespace Samples {
  public class PathsTest {
    [Test]
    public void RunScript_exists() 
      => FileAssert.Exists(Paths.RunScript);

    [Test]
    public void SamplesDir_exists() 
      => DirectoryAssert.Exists(Paths.SamplesDir);

    [Test]
    public void SampleDir_returns_the_HelloWorld_sample_dir()
      => DirectoryAssert.Exists(Paths.Sample("HelloWorld"));
  }
}