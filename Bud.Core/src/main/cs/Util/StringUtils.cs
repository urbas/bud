using System;
using System.IO;

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
  }
}