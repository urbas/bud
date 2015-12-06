using System;
using Microsoft.CodeAnalysis;
using Moq;
using NUnit.Framework;

namespace Bud.Configuration {
  public class CachingConfTest {
    private Mock<Func<Key<int>, Optional<int>>> intFunc;
    private Mock<Func<Key<object>, Optional<object>>> objectFunc;
    private CachingConf cachingConf;

    [SetUp]
    public void SetUp() {
      intFunc = new Mock<Func<Key<int>, Optional<int>>>();
      intFunc.Setup(self => self("foo")).Returns(42);
      objectFunc = new Mock<Func<Key<object>, Optional<object>>>();
      objectFunc.Setup(self => self("bar")).Returns<Key<object>>(k => new object());
      objectFunc.Setup(self => self("undefined")).Returns(new Optional<object>());
      objectFunc.Setup(self => self("defined")).Returns(new Optional<object>("42"));
      cachingConf = new CachingConf();
    }

    [Test]
    public void Delegate_to_wrapped_conf() {
      cachingConf.TryGet("foo", intFunc.Object);
      intFunc.Verify(self => self("foo"));
    }

    [Test]
    public void Return_the_value_returned_by_wrapped_conf()
      => Assert.AreEqual(42, cachingConf.TryGet("foo", intFunc.Object).Value);

    [Test]
    public void Invokes_wrapped_conf_only_once() {
      cachingConf.TryGet("bar", intFunc.Object);
      cachingConf.TryGet("bar", intFunc.Object);
      intFunc.Verify(self => self("bar"), Times.Once);
    }

    [Test]
    public void Always_returns_the_same_value()
      => Assert.AreSame(cachingConf.TryGet("bar", objectFunc.Object).Value,
                        cachingConf.TryGet("bar", objectFunc.Object).Value);

    [Test]
    public void TryGet_returns_an_empty_optional_when_the_fallback_does_not_define_the_key()
      => Assert.IsFalse(cachingConf.TryGet("undefined", objectFunc.Object)
                                   .HasValue);

    [Test]
    public void TryGet_returns_an_optional_with_a_value_when_the_key_is_defined_by_the_fallback_function()
      => Assert.IsTrue(cachingConf.TryGet("defined", objectFunc.Object)
                                  .HasValue);

    [Test]
    public void TryGet_returns_an_optional_containing_the_value_returned_by_the_fallback_function()
      => Assert.AreEqual("42",
                         cachingConf.TryGet("defined", objectFunc.Object)
                                    .Value);
  }
}