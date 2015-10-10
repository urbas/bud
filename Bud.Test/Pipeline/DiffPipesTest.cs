using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using NUnit.Framework;
using static Bud.Pipeline.Pipes;

namespace Bud.Pipeline {
  public class DiffPipesTest {
    [Test]
    public void Diffing_empty_pipe_produces_empty_diff() {
      Assert.AreEqual(Diff.Empty<int>(), Empty<Timestamped<int>>().ToDiffPipe().ToEnumerable().First());
    }

    [Test]
    public void Diffing_a_non_empty_pipe_produces_a_non_empty_diff() {
      var elements = new[] {new Timestamped<int>(42, DateTimeOffset.FromFileTime(1))};
      Assert.AreEqual(Diff.Empty<int>().NextDiff(elements),
                      ToPipe(elements).ToDiffPipe().ToEnumerable().Last());
    }

    [Test]
    public void Diffing_removal() {
      var elements1 = new[] {new Timestamped<int>(42, DateTimeOffset.FromFileTime(1))};
      var elements2 = Enumerable.Empty<Timestamped<int>>();
      Assert.AreEqual(Diff.Empty<int>().NextDiff(elements1).NextDiff(elements2),
                      ToPipe(elements1).Concat(ToPipe(elements2)).ToDiffPipe().ToEnumerable().Last());
    }

    [Test]
    public void Diffing_a_change() {
      var elements1 = new[] {new Timestamped<int>(42, DateTimeOffset.FromFileTime(1))};
      var elements2 = new[] {new Timestamped<int>(42, DateTimeOffset.FromFileTime(2))};
      Assert.AreEqual(Diff.Empty<int>().NextDiff(elements1).NextDiff(elements2),
                      ToPipe(elements1).Concat(ToPipe(elements2)).ToDiffPipe().ToEnumerable().Last());
    }
  }
}