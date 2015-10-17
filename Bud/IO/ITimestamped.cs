using System;

namespace Bud.IO {
  public interface ITimestamped {
    DateTimeOffset Timestamp { get; }
  }
}