using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Bud.Collections;
using static Bud.Option;

namespace Bud.Reactive {
  public class ObservableResults {
    private static readonly Type ObservableTypeDefinition = typeof(IObservable<object>).GetGenericTypeDefinition();
    private static readonly MethodInfo ObservableToEnumerableMethodInfo = typeof(Observable).GetMethod(nameof(Observable.ToEnumerable));

    private static readonly MethodInfo ObservableTakeMethodInfo
      = typeof(Observable).GetMethods()
                          .First(m => string.Equals(m.Name, nameof(Observable.Take))
                                      && m.GetParameters().Length == 2
                                      && m.GetParameters()[1].ParameterType == typeof(int));

    public static Option<IEnumerable<object>> TryCollect(object observable)
      => GetObservedType(observable).Map(t => ObservableToEnumerable(observable, t));

    public static Option<object> TryTakeOne(object observable)
      => GetObservedType(observable)
        .Map(t => ObservableToEnumerable(TakeOne(observable, t), t))
        .Map(e => e.TryGetFirst())
        .Flatten();

    public static Option<Type> GetObservedType(object obj)
      => obj?.GetType()
             .GetInterfaces()
             .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == ObservableTypeDefinition)
             .Select(t => Some(t.GetGenericArguments()[0]))
             .FirstOrDefault() ??
         None<Type>();

    private static IEnumerable<object> ObservableToEnumerable(object observable, Type observedType)
      => ObservableToEnumerableRaw(observable, observedType)?
        .Cast<object>()
        .ToList();

    private static object TakeOne(object observable, Type observedType)
      => ObservableTakeMethodInfo
        .MakeGenericMethod(observedType)
        .Invoke(null, new[] {observable, 1});

    private static IEnumerable ObservableToEnumerableRaw(object observable, Type observedType)
      => (IEnumerable) ObservableToEnumerableMethodInfo
                         .MakeGenericMethod(observedType)
                         .Invoke(null, new[] {observable});
  }
}