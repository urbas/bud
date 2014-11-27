using System;
using System.Collections.Immutable;

namespace Bud.Settings.Construction
{
  internal class ConfigModification<T> : Setting
  {
    public ConfigModification(ConfigKey<T> key, ConfigValueModification<T> value) : base(key, value) { }
	}


}

