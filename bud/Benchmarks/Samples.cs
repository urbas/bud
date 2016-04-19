using System.Collections.Immutable;
using System.Diagnostics;

namespace Bud.Benchmarks {
  public static class Samples {
    public static ImmutableDictionary<string, object> ToSample(Process executionInfo)
      => ImmutableDictionary<string, object>.Empty
        .Add("TotalTime", executionInfo.TotalProcessorTime)
        .Add("UserTime", executionInfo.UserProcessorTime)
        .Add("SystemTime", executionInfo.PrivilegedProcessorTime);

    public static ImmutableDictionary<string, object> Empty
      => ImmutableDictionary<string, object>.Empty;

    public static ImmutableList<ImmutableDictionary<string, object>> None =
      ImmutableList<ImmutableDictionary<string, object>>.Empty;
  }
}