using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Bud.SettingsConstruction
{
	public static class ScopeUtils
	{
    public static bool AreEqual(ImmutableList<ISettingKey> scopeA, ImmutableList<ISettingKey> scopeB) {
      if (scopeA.Count != scopeB.Count) {
        return false;
      }
      var scopeAEnumerator = scopeA.GetEnumerator();
      foreach (var subScope in scopeB) {
        scopeAEnumerator.MoveNext();
        if (!subScope.Equals(scopeAEnumerator.Current)) {
          return false;
        }
      }
      return true;
    }
	}
}
