using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Bud.IO {
  public class DiffTest {
    [Test]
    public void Empty_diff_always_returns_the_same_instance()
      => Assert.AreSame(Diff.Empty<Hashed<int>>(), Diff.Empty<Hashed<int>>());

    [Test]
    public void Empty_diff_is_empty() {
      Assert.IsEmpty(Diff.Empty<Hashed<int>>().Added);
      Assert.IsEmpty(Diff.Empty<Hashed<int>>().Removed);
      Assert.IsEmpty(Diff.Empty<Hashed<int>>().Changed);
      Assert.IsEmpty(Diff.Empty<Hashed<int>>().All);
    }

    [Test]
    public void Adding_an_element() {
      var timestampedValue = Hashed.Create(42, 1);
      var diff = Diff.Empty<Hashed<int>>()
                     .NextDiff(new[] {timestampedValue});

      Assert.That(diff.Added, Is.EquivalentTo(new[] {timestampedValue}));
      Assert.IsEmpty(diff.Removed);
      Assert.IsEmpty(diff.Changed);
      Assert.That(diff.All, Is.EquivalentTo(new[] {timestampedValue}));
    }

    [Test]
    public void Removing_an_element() {
      var removedElement = new Hashed<int>(42, 1);
      var diff = Diff.Empty<Hashed<int>>()
                     .NextDiff(new[] {removedElement})
                     .NextDiff(Enumerable.Empty<Hashed<int>>());

      Assert.IsEmpty(diff.Added);
      Assert.That(diff.Removed, Is.EquivalentTo(new[] {removedElement}));
      Assert.IsEmpty(diff.Changed);
      Assert.IsEmpty(diff.All);
    }

    [Test]
    public void Enumerate_the_timestamped_elements_only_once() {
      var timestampedElements = new Mock<IEnumerable<Hashed<int>>>(MockBehavior.Strict);
      timestampedElements.Setup(self => self.GetEnumerator()).Returns(Enumerable.Empty<Hashed<int>>().GetEnumerator());
      Diff.Empty<Hashed<int>>().NextDiff(timestampedElements.Object);
      timestampedElements.Verify(self => self.GetEnumerator(), Times.Once);
    }

    [Test]
    public void Changing_an_element() {
      var oldElement = new Hashed<int>(42, 1);
      var newElement = new Hashed<int>(42, 2);
      var diff = Diff.Empty<Hashed<int>>()
                     .NextDiff(new[] {oldElement})
                     .NextDiff(new[] {newElement});

      Assert.IsEmpty(diff.Added);
      Assert.IsEmpty(diff.Removed);
      Assert.That(diff.Changed, Is.EquivalentTo(new[] {newElement}));
      Assert.That(diff.All, Is.EquivalentTo(new[] {newElement}));
    }
  }
}