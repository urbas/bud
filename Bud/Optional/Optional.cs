namespace Bud.Optional {
  public struct Optional<T> {
    public T Value { get; }
    public bool HasValue { get; }

    public Optional(T value) {
      Value = value;
      HasValue = true;
    }

    public static implicit operator Optional<T>(T value)
      => new Optional<T>(value);
  }
}