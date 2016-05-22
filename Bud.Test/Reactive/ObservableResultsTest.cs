using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using NUnit.Framework;
using static NUnit.Framework.Assert;
using static Bud.Option;

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
    public void TryTakeOne_returns_None_when_given_null()
      => AreEqual(None<object>(),
                  ObservableResults.TryTakeOne(null));

    [Test]
    public void TryTakeOne_returns_None_when_given_an_empty_observable()
      => AreEqual(None<object>(),
                  ObservableResults.TryTakeOne(Observable.Empty<object>()));

    [Test]
    public void TryTakeOne_returns_first_value_from_given_observed_strings()
      => AreEqual("a",
                  ObservableResults.TryTakeOne(new[] { "a", "b" }.ToObservable()).Value);

    [Test]
    public void TryTakeOne_returns_first_value_from_given_observable()
      => AreEqual(42,
                  ObservableResults.TryTakeOne(new[] { 42, 1 }.ToObservable()).Value);

    [Test]
    public void GetObservedType_returns_None_when_parameter_is_not_observable()
      => AreEqual(None<Type>(),
                  ObservableResults.GetObservedType(42));

    [Test]
    public void GetObservedType_returns_int_type_when_parameter_is_observable_of_int()
      => AreEqual(Some(typeof(int)),
                  ObservableResults.GetObservedType(Observable.Return(42)));

    [Test]
    public void GetObservedType_returns_None_when_parameter_is_null()
      => AreEqual(None<Type>(),
                  ObservableResults.GetObservedType(null));
  }
}