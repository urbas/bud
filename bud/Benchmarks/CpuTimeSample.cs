using System;
using System.Diagnostics;

namespace Bud.Benchmarks {
  public class CpuTimeSample : ISample {
    public CpuTimeSample(TimeSpan totalTime, TimeSpan userTime, TimeSpan systemTime) {
      TotalTime = totalTime;
      UserTime = userTime;
      SystemTime = systemTime;
    }

    public TimeSpan TotalTime { get; }
    public TimeSpan UserTime { get; }
    public TimeSpan SystemTime { get; }

    public static CpuTimeSample ToSample(Process executionInfo)
      => new CpuTimeSample(executionInfo.TotalProcessorTime,
                           executionInfo.UserProcessorTime,
                           executionInfo.PrivilegedProcessorTime);
  }
}