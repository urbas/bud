using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using NUnit.Framework;
using static Bud.Util.Option;
using static NUnit.Framework.Assert;

namespace Bud.Reactive {
  public class ObservableResultsTest {
    [Test]
    public void TryCollect_returns_None_when_given_null()
      => AreEqual(None<IEnumerable<object>>(),
                  ObservableResults.TryCollect(null));

    [Test]
    public void TryCollect_returns_all_observed_strings()
      => AreEqual(new[] {"a", "b"},
                  ObservableResults.TryCollect(new[] {"a", "b"}.ToObservable()).Value);

    [Test]
    public void TryCollect_returns_all_observed_value_types()
      => AreEqual(new[] {1, 42},
                  ObservableResults.TryCollect(new[] {1, 42}.ToObservable()).Value);

    [Test]
    public void IsObservable_returns_false_when_parameter_is_not_observable()
      => AreEqual(None<Type>(),
                  ObservableResults.GetObservedType(42));

    [Test]
    public void IsObservable_returns_true_when_parameter_is_observable()
      => AreEqual(Some(typeof(int)),
                  ObservableResults.GetObservedType(Observable.Return(42)));

    [Test]
    public void IsObservable_returns_false_when_parameter_is_null()
      => AreEqual(None<Type>(),
                  ObservableResults.GetObservedType(null));
  }
}