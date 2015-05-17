using System;
using System.IO;
using System.Linq;

internal static class FileProcessingUtils {
  public static void ReplaceLinesInFile(string file,
                                        params Func<string, string>[] lineReplacers) {
    var lineReplacer = lineReplacers.Aggregate(ComposeFunctions);
    var replacedLines = File.ReadAllLines(file).Select(lineReplacer);
    File.WriteAllLines(file, replacedLines);
  }

  private static Func<T, T> ComposeFunctions<T>(Func<T, T> firstFunctionToApply,
                                                Func<T, T> secondFunctionToApply) {
    return line => secondFunctionToApply(firstFunctionToApply(line));
  }
}