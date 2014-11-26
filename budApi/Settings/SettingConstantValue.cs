using System;
using System.Collections.Immutable;

namespace Bud.Settings
{

  internal class SettingConstantValue : SettingValue
  {
    public readonly ImmutableHashSet<string> Value;

    public SettingConstantValue(ImmutableHashSet<string> value) {
      this.Value = value;
    }
	}


}

