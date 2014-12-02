using System;
using System.Text;

namespace Bud.Util {
  public static class SettingKeyUtils {
    public static String KeyAsString(ISettingKey key) {
      return AppendKeyAsString(new StringBuilder(), key).ToString();
    }

    public static StringBuilder AppendKeyAsString(StringBuilder sb, ISettingKey key) {
      var curScope = key.Scope;
      while (!curScope.IsGlobal) {
        sb.Append(curScope.Id).Append(':');
        curScope = curScope.Parent;
      }
      return sb.Append(key.Id);
    }
  }
}

