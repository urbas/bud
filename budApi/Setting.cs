using System;
using System.Collections.Generic;

namespace Bud
{
	public interface Setting
  {
    SettingKey Key { get; }
    SettingValue Value { get; }
	}
}

