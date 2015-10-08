using System;

namespace Bud.IO {
  public interface IFileTimestamps {
    DateTime GetTimestamp(string file);
  }
}