using System.Collections.Immutable;
using System.Diagnostics;

namespace Bud.Benchmarks {
  public static class Samples {
    public static ImmutableDictionary<string, object> ToSample(Process executionInfo)
      => ImmutableDictionary<string, object>
        .Empty
        .Add("TotalTime", executionInfo.TotalProcessorTime)
        .Add("UserTime", executionInfo.UserProcessorTime)
        .Add("SystemTime", executionInfo.PrivilegedProcessorTime);

    public static ImmutableList<ImmutableDictionary<string, object>>
      SampleList(params ImmutableDictionary<string, object>[] samples)
      => ImmutableList.CreateRange(samples);

    public static ImmutableDictionary<string, object>
      DataPoint(string datapointName, object datapointValue)
      => ImmutableDictionary<string, object>.Empty.Add(datapointName, datapointValue);
  }
}