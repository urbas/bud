using System;

namespace Bud.Util {
  public static class StringUtils {
    public static string Capitalize(string str) {
      return string.IsNullOrEmpty(str) ? str : (Char.IsLower(str[0]) ? (Char.ToUpper(str[0]) + str.Substring(1)) : str);
    }
  }
}