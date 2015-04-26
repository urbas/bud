using System;
using System.Collections.Generic;

namespace Bud.Util {
  public static class ExceptionUtils {

    public static void PrintItemizedErrorMessages(IEnumerable<Exception> exceptions, int depth) {
      foreach (var exception in exceptions) {
        var aggregateException = exception as AggregateException;
        if (aggregateException != null) {
          PrintItemizedErrorMessages(aggregateException.InnerExceptions, depth);
        } else if (exception.InnerException != null) {
          PrintErrorMessageItem(depth, exception);
          PrintItemizedErrorMessages(new[] {exception.InnerException}, depth + 1);
        } else {
          PrintErrorMessageItem(depth, exception);
        }
      }
    }

    private static void PrintErrorMessageItem(int depth, Exception exception) {
      for (int i = 0; i <= depth; i++) {
        Console.Error.Write(" ");
      }
      Console.Error.Write("- ");
      Console.Error.WriteLine(exception.Message);
    }
  }
}