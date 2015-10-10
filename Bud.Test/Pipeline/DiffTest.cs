using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using Moq;
using NUnit.Framework;

namespace Bud.Pipeline {
  public class DiffTest {
    [Test]
    public void Empty_diff_always_returns_the_same_instance()
      => Assert.AreSame(Diff.Empty<string>(), Diff.Empty<string>());

    [Test]
    public void Empty_diff_is_empty() {
      Assert.IsEmpty(Diff.Empty<string>().Added);
      Assert.IsEmpty(Diff.Empty<string>().Removed);
      Assert.IsEmpty(Diff.Empty<string>().Changed);
      Assert.IsEmpty(Diff.Empty<string>().All);
      Assert.IsEmpty(Diff.Empty<string>().Timestamps);
    }

    [Test]
    public void Adding_an_element() {
      var diff = Diff.Empty<int>();
      diff = diff.NextDiff(new[] {new Timestamped<int>(42, DateTimeOffset.FromFileTime(1))});

      Assert.That(diff.Added, Is.EquivalentTo(new[] {42}));
      Assert.IsEmpty(diff.Removed);
      Assert.IsEmpty(diff.Changed);
      Assert.That(diff.All, Is.EquivalentTo(new[] {42}));
      Assert.That(diff.Timestamps, Is.EquivalentTo(new[] {new KeyValuePair<int, DateTimeOffset>(42, DateTimeOffset.FromFileTime(1))}));
    }

    [Test]
    public void Removing_an_element() {
      var diff = Diff.Empty<string>();
      diff = diff.NextDiff(new[] {new Timestamped<string>("a", DateTimeOffset.FromFileTime(1))});
      diff = diff.NextDiff(new Timestamped<string>[] {});

      Assert.IsEmpty(diff.Added);
      Assert.That(diff.Removed, Is.EquivalentTo(new[] {"a"}));
      Assert.IsEmpty(diff.Changed);
      Assert.IsEmpty(diff.All);
      Assert.IsEmpty(diff.Timestamps);
    }

    [Test]
    public void Changing_an_element() {
      var diff = Diff.Empty<string>();
      diff = diff.NextDiff(new[] {new Timestamped<string>("a", DateTimeOffset.FromFileTime(1))});
      diff = diff.NextDiff(new[] {new Timestamped<string>("a", DateTimeOffset.FromFileTime(2))});

      Assert.IsEmpty(diff.Added);
      Assert.IsEmpty(diff.Removed);
      Assert.That(diff.Changed, Is.EquivalentTo(new[] {"a"}));
      Assert.That(diff.All, Is.EquivalentTo(new[] {"a"}));
      Assert.That(diff.Timestamps, Is.EquivalentTo(new[] {new KeyValuePair<string, DateTimeOffset>("a", DateTimeOffset.FromFileTime(2))}));
    }

    [Test]
    public void Enumerate_the_timestamped_elements_only_once() {
      var timestampedElements = new Mock<IEnumerable<Timestamped<int>>>(MockBehavior.Strict);
      timestampedElements.Setup(self => self.GetEnumerator()).Returns(Enumerable.Empty<Timestamped<int>>().GetEnumerator());
      Diff.Empty<int>().NextDiff(timestampedElements.Object);
      timestampedElements.Verify(self => self.GetEnumerator(), Times.Once);
    }
  }
}