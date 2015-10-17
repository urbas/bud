using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Bud.IO {
  public class DiffTest {
    [Test]
    public void Empty_diff_always_returns_the_same_instance()
      => Assert.AreSame(Diff.Empty<Timestamped<string>>(), Diff.Empty<Timestamped<string>>());

    [Test]
    public void Empty_diff_is_empty() {
      Assert.IsEmpty(Diff.Empty<Timestamped<string>>().Added);
      Assert.IsEmpty(Diff.Empty<Timestamped<string>>().Removed);
      Assert.IsEmpty(Diff.Empty<Timestamped<string>>().Changed);
      Assert.IsEmpty(Diff.Empty<Timestamped<string>>().All);
    }

    [Test]
    public void Adding_an_element() {
      var timestampedValue = Timestamped.Create(42, DateTimeOffset.FromFileTime(1));
      var diff = Diff.Empty<Timestamped<int>>()
                     .NextDiff(new[] {timestampedValue});

      Assert.That(diff.Added, Is.EquivalentTo(new[] {timestampedValue}));
      Assert.IsEmpty(diff.Removed);
      Assert.IsEmpty(diff.Changed);
      Assert.That(diff.All, Is.EquivalentTo(new[] {timestampedValue}));
    }

    [Test]
    public void Removing_an_element() {
      var removedElement = new Timestamped<int>(42, DateTimeOffset.FromFileTime(1));
      var diff = Diff.Empty<Timestamped<int>>()
                     .NextDiff(new[] {removedElement})
                     .NextDiff(Enumerable.Empty<Timestamped<int>>());

      Assert.IsEmpty(diff.Added);
      Assert.That(diff.Removed, Is.EquivalentTo(new[] {removedElement}));
      Assert.IsEmpty(diff.Changed);
      Assert.IsEmpty(diff.All);
    }

    [Test]
    public void Enumerate_the_timestamped_elements_only_once() {
      var timestampedElements = new Mock<IEnumerable<Timestamped<int>>>(MockBehavior.Strict);
      timestampedElements.Setup(self => self.GetEnumerator()).Returns(Enumerable.Empty<Timestamped<int>>().GetEnumerator());
      Diff.Empty<Timestamped<int>>().NextDiff(timestampedElements.Object);
      timestampedElements.Verify(self => self.GetEnumerator(), Times.Once);
    }

    [Test]
    public void Changing_an_element() {
      var oldElement = new Timestamped<int>(42, DateTimeOffset.FromFileTime(1));
      var newElement = new Timestamped<int>(42, DateTimeOffset.FromFileTime(2));
      var diff = Diff.Empty<Timestamped<int>>()
                     .NextDiff(new[] {oldElement})
                     .NextDiff(new[] {newElement});

      Assert.IsEmpty(diff.Added);
      Assert.IsEmpty(diff.Removed);
      Assert.That(diff.Changed, Is.EquivalentTo(new[] {newElement}));
      Assert.That(diff.All, Is.EquivalentTo(new[] {newElement}));
    }
  }
}