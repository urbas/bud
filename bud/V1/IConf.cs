namespace Bud.V1 {
  /// <summary>
  ///   This is interface provides a bag of key-to-configuration pairs.
  ///   You can retrieve the values of configurations via <see cref="TryGet{T}" />.
  /// </summary>
  public interface IConf {
    /// <typeparam name="T">
    ///   the type of the value of the configuration pointed to by the key
    /// </typeparam>
    /// <param name="key">
    ///   a slash-spearated path like "<c>foo/bar/A</c>".
    /// </param>
    /// <returns>
    ///   If the configuration at the given key is defined, then
    ///   this function returns the value of the configuration. If the
    ///   configuration is not defined, this method returns
    ///   <see cref="Bud.Option.None{T}" />
    /// </returns>
    /// <remarks>
    /// </remarks>
    Option<T> TryGet<T>(Key<T> key);

    /// <summary>
    ///   The key of the current context.
    /// </summary>
    /// <remarks>
    ///   If you're using this conf from a configuration, then this
    ///   is the key of that configuration.
    /// </remarks>
    Key Key { get; }
  }
}