using Microsoft.CodeAnalysis;

namespace Bud.Util {
  public static class OptionalExtensions {
    public static T GetOrElse<T>(this Optional<T> optional, T defaultValue)
      => optional.HasValue ? optional.Value : defaultValue;
  }
}