using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Bud.SettingsConstruction;
using Bud.Util;

namespace Bud {

  public abstract class TaskKey : Key {
    protected TaskKey(string id) : base(id) {}
    protected TaskKey(string id, Key parent) : base(id, parent) {}
  }

  /// <summary>
  /// Values of this key are evaluated once per evaluation context.
  /// </summary>
  public class TaskKey<T> : TaskKey {
    public TaskKey(string id) : base(id) {}

    private TaskKey(string id, Key parent) : base(id, parent) {}

    public new TaskKey<T> In(Key parent) {
      if (parent.IsGlobal) {
        return this;
      }
      return new TaskKey<T>(Id, Concat(parent, Parent));
    }
  }
}

