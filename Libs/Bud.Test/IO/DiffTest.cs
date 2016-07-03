using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Bud.IO {
  public class DiffTest {
    [Test]
    public void Empty_diff_always_returns_the_same_instance()
      => Assert.AreSame(Diff.Empty<Timestamped<int>>(), Diff.Empty<Timestamped<int>>());

    [Test]
    public void Empty_diff_is_empty() {
      Assert.IsEmpty(Diff.Empty<Timestamped<int>>().Added);
      Assert.IsEmpty(Diff.Empty<Timestamped<int>>().Removed);
      Assert.IsEmpty(Diff.Empty<Timestamped<int>>().Changed);
      Assert.IsEmpty(Diff.Empty<Timestamped<int>>().All);
    }

    [Test]
    public void Adding_an_element() {
      var timestampedValue = Timestamped.Create(42, 1);
      var diff = SingleElementDiff(timestampedValue);

      Assert.That(diff.Added, Is.EquivalentTo(new[] {timestampedValue}));
      Assert.IsEmpty(diff.Removed);
      Assert.IsEmpty(diff.Changed);
      Assert.That(diff.All, Is.EquivalentTo(new[] {timestampedValue}));
    }

    [Test]
    public void Removing_an_element() {
      var removedElement = new Timestamped<int>(42, 1);
      var diff = Diff.Empty<Timestamped<int>>()
                     .DiffByTimestamp(new[] {removedElement})
                     .DiffByTimestamp(Enumerable.Empty<Timestamped<int>>());

      Assert.IsEmpty(diff.Added);
      Assert.That(diff.Removed, Is.EquivalentTo(new[] {removedElement}));
      Assert.IsEmpty(diff.Changed);
      Assert.IsEmpty(diff.All);
    }

    [Test]
    public void Enumerate_the_timestamped_elements_only_once() {
      var timestampedElements = new Mock<IEnumerable<Timestamped<int>>>(MockBehavior.Strict);
      timestampedElements.Setup(self => self.GetEnumerator()).Returns(Enumerable.Empty<Timestamped<int>>().GetEnumerator());
      Diff.Empty<Timestamped<int>>().DiffByTimestamp(timestampedElements.Object);
      timestampedElements.Verify(self => self.GetEnumerator(), Times.Once);
    }

    [Test]
    public void Changing_an_element() {
      var oldElement = new Timestamped<int>(42, 1);
      var newElement = new Timestamped<int>(42, 2);
      var diff = SingleElementDiff(oldElement, newElement);
      Assert.IsEmpty(diff.Added);
      Assert.IsEmpty(diff.Removed);
      Assert.That(diff.Changed, Is.EquivalentTo(new[] {newElement}));
      Assert.That(diff.All, Is.EquivalentTo(new[] {newElement}));
    }

    [Test]
    public void Hash_codes_must_equal_for_equal_diffs() {
      var oldElement = Timestamped.Create(42, 1);
      var newElement = Timestamped.Create(42, 2);
      var diff1 = SingleElementDiff(oldElement, newElement);
      var diff2 = SingleElementDiff(oldElement, newElement);
      Assert.AreEqual(diff1.GetHashCode(), diff2.GetHashCode());
    }

    private static Diff<Timestamped<T>> SingleElementDiff<T>(params Timestamped<T>[] elementHistory)
      => elementHistory.Aggregate(Diff.Empty<Timestamped<T>>(),
                                  (diff, nextElement) => diff.DiffByTimestamp(new[] {nextElement}));
  }
}