using System.Collections.Immutable;
using Bud.V1;
using Moq;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Bud.Configuration {
  public class PerKeyConfTest {
    private readonly ImmutableList<string> fooBarScope = ImmutableList.Create("foo", "bar");
    private Mock<IConf> wrappedConf;
    private IConf scopedConf;

    [SetUp]
    public void SetUp() {
      wrappedConf = new Mock<IConf>();
      wrappedConf.Setup(self => self.TryGet<int>("foo/A")).Returns(42);
      scopedConf = new PerKeyConf(wrappedConf.Object, fooBarScope, "a/b");
    }

    [Test]
    public void Delegate_absolute_keys() {
      scopedConf.TryGet<int>("/A");
      wrappedConf.Verify(self => self.TryGet<int>("/A"));
    }

    [Test]
    public void Delegate_relative_keys() {
      scopedConf.TryGet<int>("../A");
      wrappedConf.Verify(self => self.TryGet<int>("foo/A"));
    }

    [Test]
    public void Delegate_keys_that_go_out_of_scope() {
      scopedConf.TryGet<int>("../../../A");
      wrappedConf.Verify(self => self.TryGet<int>("../A"));
    }

    [Test]
    public void Return_value_calculated_by_wrapped_conf()
      => AreEqual(42, scopedConf.TryGet<int>("../A").Value);

    [Test]
    public void Key_is_set() => AreEqual(new Key("a/b"), scopedConf.Key);

    [Test]
    public void Key_is_set_when_dir_is_not_set() {
      var conf = (IConf) new PerKeyConf((IConf) wrappedConf.Object, ImmutableList<string>.Empty, "a/b");
      AreEqual(new Key("a/b"), conf.Key);
    }
  }
}