using System.Collections.Immutable;
using Moq;
using NUnit.Framework;

namespace Bud.Configuration {
  public class ScopedConfTest {
    private readonly ImmutableList<string> fooBarScope = ImmutableList.Create("foo", "bar");
    private Mock<IConf> wrappedConf;
    private IConf scopedConf;

    [SetUp]
    public void SetUp() {
      wrappedConf = new Mock<IConf>();
      wrappedConf.Setup(self => self.Get<int>("foo/A")).Returns(42);
      scopedConf = ScopedConf.MakeScoped(fooBarScope, wrappedConf.Object);
    }

    [Test]
    public void Do_not_wrap_conf_if_scope_is_empty()
      => Assert.AreSame(wrappedConf.Object, ScopedConf.MakeScoped(ImmutableList<string>.Empty, wrappedConf.Object));

    [Test]
    public void Wrap_if_the_scope_is_non_empty()
      => Assert.AreNotSame(wrappedConf.Object, ScopedConf.MakeScoped(fooBarScope, wrappedConf.Object));

    [Test]
    public void Delegate_absolute_keys() {
      scopedConf.Get<int>("/A");
      wrappedConf.Verify(self => self.Get<int>("/A"));
    }

    [Test]
    public void Delegate_relative_keys() {
      scopedConf.Get<int>("../A");
      wrappedConf.Verify(self => self.Get<int>("foo/A"));
    }

    [Test]
    public void Delegate_keys_that_go_out_of_scope() {
      scopedConf.Get<int>("../../../A");
      wrappedConf.Verify(self => self.Get<int>("../A"));
    }

    [Test]
    public void Return_value_calculated_by_wrapped_conf()
      => Assert.AreEqual(42, scopedConf.Get<int>("../A"));
  }
}