using NUnit.Framework;
using static NUnit.Framework.Assert;
using static Bud.Option;

namespace Bud.NuGet {
  public class NuGetPushCliArgsBuilderTest {
    private NuGetPushArgsBuilder argsBuilder;

    [SetUp]
    public void SetUp() => argsBuilder = new NuGetPushArgsBuilder();

    [Test]
    public void CreateArgs_with_no_optional_arguments()
      => AreEqual("push foo",
                  argsBuilder.CreateArgs("foo", None<string>(), None<string>()));

    [Test]
    public void CreateArgs_with_all_optional_arguments()
      => AreEqual("push foo -Source bar -ApiKey zar",
                  argsBuilder.CreateArgs("foo", "bar", "zar"));
  }
}