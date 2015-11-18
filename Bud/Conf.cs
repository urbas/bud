using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.Configuration;

namespace Bud {
  public class Conf : IConf, IConfBuilder {
    public static Conf Empty { get; } = new Conf(ImmutableList<ScopedConfBuilder>.Empty, ImmutableList<string>.Empty);
    public ImmutableList<ScopedConfBuilder> ScopedConfBuilders { get; }
    public ImmutableList<string> Scope { get; }

    public Conf(ImmutableList<ScopedConfBuilder> scopedConfBuilders, ImmutableList<string> scope) {
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
      => new Conf(ScopedConfBuilders.AddRange(otherConfs.Select(MakeScopedConfBuilder)),
                  Scope);

    /// <summary>
    ///   TODO: Remove. Consider creating a separate helper API for collections.
    /// </summary>
    public Conf Add<T>(Key<IEnumerable<T>> dependencies, params T[] v)
      => Modify(dependencies, (conf, enumerable) => enumerable.Concat(v));

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

    /// <returns>the value of the configuration key.</returns>
    public T Get<T>(Key<T> key)
      => ToCachingConf().Get<T>(Keys.InterpretFromScope(key, Scope));

    public void ApplyIn(ScopedDictionaryBuilder<IConfDefinition> configDefinitions) {
      foreach (var scopedConfBuilder in ScopedConfBuilders) {
        scopedConfBuilder.ConfBuilder
                         .ApplyIn(configDefinitions.In(scopedConfBuilder.Scope));
      }
    }

    public CachingConf ToCachingConf() {
      var definitions = new Dictionary<string, IConfDefinition>();
      ApplyIn(new ScopedDictionaryBuilder<IConfDefinition>(definitions, ImmutableList<string>.Empty));
      return new CachingConf(new ConfValueCalculator(definitions));
    }

    public static Conf InConf(string scope)
      => new Conf(ImmutableList<ScopedConfBuilder>.Empty, ImmutableList.Create(scope));

    public static Conf Group(params IConfBuilder[] confs)
      => Group((IEnumerable<IConfBuilder>) confs);

    public static Conf Group(IEnumerable<IConfBuilder> confs)
      => Empty.Add(confs);

    private ScopedConfBuilder MakeScopedConfBuilder(IConfBuilder builder)
      => new ScopedConfBuilder(Scope, builder);
  }
}