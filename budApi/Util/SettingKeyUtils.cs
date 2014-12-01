using System;
using System.Text;

namespace Bud.Util {
  public static class SettingKeyUtils {
    public static String KeyAsString(ISettingKey key) {
      return AppendKeyAsString(new StringBuilder(), key).ToString();
    }

    public static StringBuilder AppendKeyAsString(StringBuilder sb, ISettingKey key) {
      if (!key.Scope.IsEmpty) {
        foreach (var subScope in key.Scope) {
          if (subScope.Scope.IsEmpty) {
            AppendKeyAsString(sb, subScope).Append(':');
          } else {
            sb.Append('{');
            AppendKeyAsString(sb, subScope);
            sb.Append("}:");
          }
        }
      }
      return sb.Append(key.Id);
    }
  }
}

