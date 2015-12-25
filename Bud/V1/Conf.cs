using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.Configuration;
using Bud.Optional;

namespace Bud.V1 {
  public class Conf : IConfBuilder {
    public static Conf Empty { get; } = new Conf(ImmutableList<ScopedConfBuilder>.Empty, ImmutableList<string>.Empty);
    private ImmutableList<ScopedConfBuilder> ScopedConfBuilders { get; }
    public ImmutableList<string> Scope { get; }

    private Conf(ImmutableList<ScopedConfBuilder> scopedConfBuilders, ImmutableList<string> scope) {
      ScopedConfBuilders = scopedConfBuilders;
      Scope = scope;
    }

    /// <summary>
    ///   Defines a constant-valued configuration.
    ///   If the configuration is already defined, then this method overwrites it.
    /// </summary>
    public Conf SetValue<T>(Key<T> configKey, T value)
      => Set(configKey, cfg => value);

    /// <summary>
    ///   Defines a constant-valued configuration.
    ///   If the configuration is already defined, then this method does nothing.
    /// </summary>
    public Conf InitValue<T>(Key<T> configKey, T value)
      => Init(configKey, cfg => value);

    /// <summary>
    ///   Defines a configuration that returns the value produced by <paramref name="valueFactory" />.
    ///   <paramref name="valueFactory" /> is invoked when the configuration is accessed.
    ///   If the configuration is already defined, then this method does nothing.
    /// </summary>
    public Conf Init<T>(Key<T> configKey, Func<IConf, T> valueFactory)
      => Add(new InitConf<T>(configKey, valueFactory));

    /// <summary>
    ///   Defines a configuration that returns the value produced by <paramref name="valueFactory" />.
    ///   <paramref name="valueFactory" /> is invoked when the configuration is accessed.
    ///   If the configuration is already defined, then this method overwrites the configuration.
    /// </summary>
    public Conf Set<T>(Key<T> configKey, Func<IConf, T> valueFactory)
      => Add(new SetConf<T>(configKey, valueFactory));

    /// <summary>
    ///   Defines a configuration that returns the value produced by <paramref name="valueFactory" />.
    ///   <paramref name="valueFactory" /> is invoked when the configuration is accessed.
    ///   If the configuration is already defined, then this method overwrites the configuration.
    /// </summary>
    public Conf Modify<T>(Key<T> configKey, Func<IConf, T, T> valueFactory)
      => Add(new ModifyConf<T>(configKey, valueFactory));

    /// <returns>a copy of self with added configurations from <paramref name="otherConfs" />.</returns>
    public Conf Add(params IConfBuilder[] otherConfs)
      => Add((IEnumerable<IConfBuilder>) otherConfs);

    public Conf Add(IEnumerable<IConfBuilder> otherConfs)
      => new Conf(ScopedConfBuilders.AddRange(ToScopedConfBuilders(otherConfs, Scope)),
                  Scope);

    /// <returns>
    ///   a copy of self where the Scope is appended with <paramref name="scope" />.
    /// </returns>
    public Conf In(string scope)
      => new Conf(ScopedConfBuilders, Scope.Add(scope));

    /// <returns>
    ///   a copy of self where the Scope has one element removed from the back.
    /// </returns>
    public Conf Out() {
      if (Scope.IsEmpty) {
        throw new InvalidOperationException("Can not backtrack further. The Scope is already empty.");
      }
      return new Conf(ScopedConfBuilders, Scope.RemoveAt(Scope.Count - 1));
    }

    /// <param name="key">
    ///   The key for which to get the value. If the path of the key is relative,
    ///   it will be interpreted with the <see cref="Scope" /> as the base path.
    /// </param>
    public Optional<T> TryGet<T>(Key<T> key)
      => ToCompiled().TryGet<T>(Keys.InterpretFromScope(key, Scope));

    public void ApplyIn(ScopedDictionaryBuilder<IConfDefinition> configDefinitions) {
      foreach (var scopedConfBuilder in ScopedConfBuilders) {
        scopedConfBuilder.ConfBuilder
                         .ApplyIn(configDefinitions.In(scopedConfBuilder.Scope));
      }
    }

    public IConf ToCompiled() {
      var definitions = new Dictionary<string, IConfDefinition>();
      ApplyIn(new ScopedDictionaryBuilder<IConfDefinition>(definitions, ImmutableList<string>.Empty));
      return new RawConf(definitions);
    }

    public static Conf Group(string scope, params IConfBuilder[] confs) {
      var scopeAsList = ImmutableList.Create(scope);
      return new Conf(ToScopedConfBuilders(confs, scopeAsList).ToImmutableList(),
                      scopeAsList);
    }

    public static Conf Group(params IConfBuilder[] confs)
      => Group((IEnumerable<IConfBuilder>)confs);

    public static Conf Group(IEnumerable<IConfBuilder> confs)
      => Empty.Add(confs);

    internal static IEnumerable<ScopedConfBuilder> ToScopedConfBuilders(IEnumerable<IConfBuilder> otherConfs, ImmutableList<string> scope)
      => otherConfs.Select(builder => new ScopedConfBuilder(scope, builder));
  }
}