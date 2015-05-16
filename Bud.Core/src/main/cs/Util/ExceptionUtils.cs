using System;
using System.Collections.Generic;

namespace Bud.Util {
  public static class ExceptionUtils {
    public static void PrintItemizedErrorMessages(Exception exception, bool areStackTracesEnabled) => PrintItemizedErrorMessages(new[] {exception}, areStackTracesEnabled, 0);

    public static void PrintItemizedErrorMessages(IEnumerable<Exception> exceptions, bool areStackTracesEnabled, int depth) {
      foreach (var exception in exceptions) {
        var aggregateException = exception as AggregateException;
        if (aggregateException != null) {
          PrintItemizedErrorMessages(aggregateException.InnerExceptions, areStackTracesEnabled, depth);
        } else if (exception.InnerException != null) {
          PrintErrorMessageItem(depth, exception, areStackTracesEnabled);
          PrintItemizedErrorMessages(new[] {exception.InnerException}, areStackTracesEnabled, depth + 1);
        } else {
          PrintErrorMessageItem(depth, exception, areStackTracesEnabled);
        }
      }
    }

    private static void PrintErrorMessageItem(int depth, Exception exception, bool areStackTracesEnabled) {
      for (int i = 0; i <= depth; i++) {
        Console.Error.Write(' ');
      }
      Console.Error.Write("- ");
      if (areStackTracesEnabled) {
        Console.Error.WriteLine($"{exception.Message}\n{exception.StackTrace}");
      } else {
        Console.Error.WriteLine(exception.Message);
      }
    }
  }
}