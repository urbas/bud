using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Bud.Cli;
using Bud.Plugins;
using System.Collections.Immutable;

namespace Bud
{
  public class ScopedSettings : Settings
  {
    public ISettingKey Scope;

    public ScopedSettings(ImmutableList<Setting> settings, ISettingKey scope) : base(settings) {
      this.Scope = scope;
    }
	}
}

