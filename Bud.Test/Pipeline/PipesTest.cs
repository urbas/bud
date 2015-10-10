using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Moq;
using NUnit.Framework;
using static Bud.Pipeline.Pipes;

namespace Bud.Pipeline {
  public class PipesTest {
    [Test]
    public void Empty_pipe_produces_a_single_empty_collection()
      => Assert.That(Empty<int>().ToEnumerable(),
                     Is.EquivalentTo(ImmutableArray.Create(Enumerable.Empty<int>())));

    [Test]
    public void Empty_pipe_is_reusable() {
      Empty_pipe_produces_a_single_empty_collection();
      Empty_pipe_produces_a_single_empty_collection();
    }

    [Test]
    public void Joining_empties_should_produce_a_single_update()
      => Assert.That(Empty<int>().JoinPipes(Empty<int>()).ToEnumerable(),
                     Is.EquivalentTo(ImmutableArray.Create(Enumerable.Empty<int>())));

    [Test]
    public void Joining_an_empty_with_nonempty_pipe_should_produce_a_single_non_empty_collection()
      => Assert.That(Empty<int>().JoinPipes(CreatePipe(4)).ToEnumerable(),
                     Is.EquivalentTo(ImmutableArray.Create(ImmutableArray.Create(4))));

    [Test]
    public void Joining_two_nonempties_should_produce_a_concatenated_collection()
      => Assert.That(CreatePipe(1, 2, 3).JoinPipes(CreatePipe(4, 5)).ToEnumerable(),
                     Is.EquivalentTo(ImmutableArray.Create(ImmutableArray.Create(1, 2, 3, 4, 5))));

    [Test]
    public void Joining_a_single_entry_pipe_with_a_two_entry_pipe_produces_two_entries() {
      var collectionPipe1 = CreatePipe(1);
      var collectionPipe2 = CreatePipe(2).Concat(CreatePipe(42));
      Assert.That(collectionPipe1.JoinPipes(collectionPipe2).ToEnumerable(),
                  Is.EquivalentTo(ImmutableArray.Create(ImmutableArray.Create(1, 2), ImmutableArray.Create(1, 42))));
    }

    [Test]
    public void Filter_can_exclude_everything()
      => Assert.That(CreatePipe("a").FilterPipe(s => false).ToEnumerable().First(),
                     Is.Empty);

    [Test]
    public void Filter_can_exclude_nothing()
      => Assert.That(CreatePipe("a").FilterPipe(s => true).ToEnumerable().First(),
                     Is.EquivalentTo(ImmutableArray.Create("a")));

    [Test]
    public void Filter_can_exclude_something()
      => Assert.That(CreatePipe("a", "b").FilterPipe(s => s == "b").ToEnumerable().First(),
                     Is.EquivalentTo(ImmutableArray.Create("b")));

    [Test]
    public void Pipe_produces_a_single_collection_from_factory() {
      var collection = ImmutableArray.Create(42);
      Assert.That(ToPipe(() => collection).ToEnumerable(),
                  Is.EquivalentTo(new[] {collection}));
    }

    [Test]
    public void Factory_is_not_called_when_pipe_is_not_pulled() {
      var collectionFactory = new Mock<Func<IEnumerable<int>>>(MockBehavior.Strict);
      ToPipe(collectionFactory.Object).ToEnumerable();
    }

    [Test]
    public void Factory_is_called_twice_when_pipe_is_pulled_twice() {
      var collectionFactory = new Mock<Func<IEnumerable<int>>>(MockBehavior.Strict);
      var enumerable = ToPipe(collectionFactory.Object).ToEnumerable();
      collectionFactory.Setup(self => self()).Returns(Enumerable.Empty<int>());
      enumerable.First();
      collectionFactory.Setup(self => self()).Returns(Enumerable.Empty<int>());
      enumerable.First();
    }
  }
}