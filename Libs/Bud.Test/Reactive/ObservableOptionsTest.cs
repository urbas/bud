using System.Reactive.Linq;
using NUnit.Framework;
using static Bud.Option;

namespace Bud.Reactive {
  public class ObservableOptionsTest {
    [Test]
    public void Gather_discards_none_observations()
      => Assert.AreEqual(new[] {1, 2},
                         new[] {None<int>(), Some(1), None<int>(), Some(2)}.ToObservable()
                                                                           .Gather().ToList().Wait());

    [Test]
    public void Gather_selects_and_filters_observed_values()
      => Assert.AreEqual(new[] {"1", "3"},
                         new[] {1, 2, 3, 4}.ToObservable()
                                           .Gather(IfOddToString)
                                           .ToList().Wait());

    private static Option<string> IfOddToString(int number)
      => number%2 == 1 ? number.ToString() : None<string>();
  }
}