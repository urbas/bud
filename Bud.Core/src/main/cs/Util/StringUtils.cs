using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bud.Util {
  public static class StringUtils {
    public static string Capitalize(string str) {
      return string.IsNullOrEmpty(str) ? str : (Char.IsLower(str[0]) ? (Char.ToUpper(str[0]) + str.Substring(1)) : str);
    }

    public static TextWriter ToHexString(byte[] bytes, TextWriter textWriter) {
      if (bytes == null) {
        throw new ArgumentNullException("bytes");
      }
      if (textWriter == null) {
        throw new ArgumentNullException("textWriter");
      }
      for (int i = 0; i < bytes.Length; i++) {
        textWriter.Write(GetHiHex(bytes[i]));
        textWriter.Write(GetLoHex(bytes[i]));
      }
      return textWriter;
    }

    private static char GetLoHex(byte b) {
      return NibbleToHex((byte) (b & 0x0f));
    }

    private static char GetHiHex(byte b) {
      return NibbleToHex((byte) ((b & 0xf0) >> 4));
    }

    private static char NibbleToHex(byte nibble) {
      return nibble > 9 ? (char) ('a' + (nibble - 10)) : (char) ('0' + nibble);
    }

    public static string CommonPrefix(string str1, string str2) {
      var minLength = Math.Min(str1.Length, str2.Length);
      for (int i = 0; i < minLength; i++) {
        if (str1[i] != str2[i]) {
          return str1.Substring(0, i);
        }
      }
      return str1.Substring(0, minLength);
    }

    public static string CommonPrefix(params string[] suggestions)
      => CommonPrefix((IEnumerable<string>)suggestions);

    public static string CommonPrefix(IEnumerable<string> suggestions)
      => suggestions.Any() ? suggestions.Aggregate(CommonPrefix) : string.Empty;
  }
}