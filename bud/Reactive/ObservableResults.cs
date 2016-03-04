using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Bud.Util;
using static Bud.Util.Option;

namespace Bud.Reactive {
  public class ObservableResults {
    private static readonly Type ObservableTypeDefinition = typeof(IObservable<object>).GetGenericTypeDefinition();
    private static readonly MethodInfo ObservableToEnumerableMethodInfo = typeof(Observable).GetMethod(nameof(Observable.ToEnumerable));

    public static Option<IEnumerable<object>> TryCollect(object observable)
      => observable == null ?
           None<IEnumerable<object>>() :
           GetObservedType(observable)
             .Map(t => ObservableToEnumerable(observable, t));

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

    private static IEnumerable ObservableToEnumerableRaw(object observable, Type observedType)
      => (IEnumerable) ObservableToEnumerableMethodInfo
                         .MakeGenericMethod(observedType)
                         .Invoke(null, new[] {observable});
  }
}