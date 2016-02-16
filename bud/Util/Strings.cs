namespace Bud.Util {
  public static class Strings {
    public static bool ContainsAt(string str, string subStr, int startIndex) {
      if (startIndex == 0) {
        return str.StartsWith(subStr);
      }
      if (subStr.Length > str.Length - startIndex) {
        return false;
      }
      for (int i = 0; i < subStr.Length; i++) {
        if (str[startIndex + i] != subStr[i]) {
          return false;
        }
      }
      return true;
    }
  }
}