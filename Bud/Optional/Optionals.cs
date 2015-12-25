namespace Bud.Optional {
  public static class Optionals {
    public static Optional<T> None<T>() => NoneOptional<T>.Instance;
    public static Optional<T> Some<T>(T value) => new Optional<T>(value);

    private static class NoneOptional<T> {
      public static readonly Optional<T> Instance = new Optional<T>();
    }
  }
}