using System;
using System.Collections.Immutable;

namespace Bud.Settings
{
  internal class ConfigValueModification<T> : SettingValue
  {
    public readonly Func<T, T> Modification;

    public ConfigValueModification(Func<T, T> modification) {
      this.Modification = modification;
    }
	}


}

