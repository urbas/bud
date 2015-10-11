using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using NUnit.Framework;
using static Bud.Pipeline.Streams;

namespace Bud.Pipeline {
  public class StreamsTest {
    [Test]
    public void Empty_stream_produces_a_single_empty_collection()
      => Assert.That(Empty<int>().ToEnumerable(),
                     Is.EquivalentTo(ImmutableArray.Create(Enumerable.Empty<int>())));

    [Test]
    public void Empty_stream_is_reusable() {
      Empty_stream_produces_a_single_empty_collection();
      Empty_stream_produces_a_single_empty_collection();
    }

    [Test]
    public void Joining_empties_should_produce_a_single_update()
      => Assert.That(Empty<int>().CombineStream(Empty<int>()).ToEnumerable(),
                     Is.EquivalentTo(ImmutableArray.Create(Enumerable.Empty<int>())));

    [Test]
    public void Joining_an_empty_with_nonempty_stream_should_produce_a_single_non_empty_collection()
      => Assert.That(Empty<int>().CombineStream(CreateStream(4)).ToEnumerable(),
                     Is.EquivalentTo(ImmutableArray.Create(ImmutableArray.Create(4))));

    [Test]
    public void Joining_two_nonempties_should_produce_a_concatenated_collection()
      => Assert.That(CreateStream(1, 2, 3).CombineStream(CreateStream(4, 5)).ToEnumerable(),
                     Is.EquivalentTo(ImmutableArray.Create(ImmutableArray.Create(1, 2, 3, 4, 5))));

    [Test]
    public void Joining_a_single_entry_stream_with_a_two_entry_stream_produces_two_entries() {
      var stream1 = CreateStream(1);
      var stream2 = CreateStream(2).Concat(CreateStream(42));
      Assert.That(stream1.CombineStream(stream2).ToEnumerable(),
                  Is.EquivalentTo(ImmutableArray.Create(ImmutableArray.Create(1, 2), ImmutableArray.Create(1, 42))));
    }
  }
}