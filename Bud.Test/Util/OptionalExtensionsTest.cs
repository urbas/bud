using System.Reactive.Linq;
using Bud.Optional;
using NUnit.Framework;
using static NUnit.Framework.Assert;
using static Bud.Optional.Optionals;

namespace Bud.Util {
  public class OptionalExtensionsTest {
    [Test]
    public void GetOrElse_returns_the_contained_value()
      => AreEqual(42, Some(42).GetOrElse(9001));

    [Test]
    public void GetOrElse_returns_the_default_value()
      => AreEqual(9001, None<int>().GetOrElse(9001));

    [Test]
    public void Gather_returns_values()
      => AreEqual(new[] {1, 2},
                  new[] {None<int>(), Some(1), None<int>(), Some(2)}
                    .Gather());

    [Test]
    public void Gather_filters_and_selects()
      => AreEqual(new[] {"1", "3"},
                  new[] {1, 2, 3, 4}.Gather(number => number%2 == 1 ? Some(number.ToString()) : None<string>()));

    [Test]
    public void Gather_discards_none_observations()
      => AreEqual(new[] {1, 2},
                  new[] {None<int>(), Some(1), None<int>(), Some(2)}.ToObservable()
                                                                    .Gather().ToList().Wait());

    [Test]
    public void Gather_selects_and_filters_observed_values()
      => AreEqual(new[] {"1", "3"},
                  new[] {1, 2, 3, 4}.ToObservable()
                                    .Gather(number => number%2 == 1 ? Some(number.ToString()) : None<string>()).ToList().Wait());
  }
}