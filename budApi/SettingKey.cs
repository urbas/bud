using System;
using System.Collections.Generic;

namespace Bud {
  public interface SettingKey {
  }

  /// <summary>
  /// Values of this key are evaluated once only (during build loading).
  /// </summary>
  public class ConfigKey<T> : SettingKey {
  }

  /// <summary>
  /// Values of this key are evaluated once on every build evaluation (if two task keys A and B
  /// depend on the same task key C then the value of task key C will be calculated only once).
  /// </summary>
  public class TaskKey<T> : SettingKey {
  }
}

