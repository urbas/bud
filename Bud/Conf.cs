using System;
using System.Collections.Generic;
using System.Linq;
using Bud.Configuration;

namespace Bud {
  public struct Conf : IConf, IConfBuilder {
    public static Conf Empty { get; } = new Conf(Enumerable.Empty<IConfBuilder>(), string.Empty);
    private IEnumerable<IConfBuilder> ConfBuilders { get; }
    public string Scope { get; }

    public Conf(IEnumerable<IConfBuilder> confBuilders, string scope) {
      ConfBuilders = confBuilders;
      Scope = scope;
    }

    /// <summary>
    ///   Defines a constant-valued configuration.
    ///   If the configuration is already defined, then this method overwrites it.
    /// </summary>
    public Conf SetValue<T>(Key<T> configKey, T value) => Set(configKey, cfg => value);

    /// <summary>
    ///   Defines a constant-valued configuration.
    ///   If the configuration is already defined, then this method does nothing.
    /// </summary>
    public Conf InitValue<T>(Key<T> configKey, T value) => Init(configKey, cfg => value);

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
      => new Conf(ConfBuilders.Concat(otherConfs), Scope);

    public static Conf Group(params IConfBuilder[] confBuilders)
      => Empty.Add(confBuilders);

    public static Conf Group(IEnumerable<IConfBuilder> configTransforms)
      => Empty.Add(configTransforms);

    public Conf Add<T>(Key<IEnumerable<T>> dependencies, params T[] v)
      => Modify(dependencies, (conf, enumerable) => enumerable.Concat(v));

    /// <returns>a copy of self where every configuration key is prefixed with <paramref name="scope" />.</returns>
    public Conf In(string scope)
      => new Conf(ConfBuilders, Keys.PrefixWith(scope, Scope));

    /// <returns>the value of the configuration key.</returns>
    /// <exception cref="ConfigTypeException">
    ///   thrown if the actual type of the configuration does not match the requested type <typeparamref name="T" />.
    /// </exception>
    public T Get<T>(Key<T> key)
      => ToCachingConf().Get(key);

    public void ApplyIn(IDictionary<string, IConfDefinition> configDefinitions) {
      if (string.IsNullOrEmpty(Scope)) {
        CreateUnscopedConfigDefinitions(configDefinitions);
      } else {
        CreateScopedConfigDefinitions(configDefinitions);
      }
    }

    public CachingConf ToCachingConf()
      => new CachingConf(CreateUnscopedConfigDefinitions());

    private IDictionary<string, IConfDefinition> CreateUnscopedConfigDefinitions(IDictionary<string, IConfDefinition> configDefinitions = null) {
      configDefinitions = configDefinitions ?? new Dictionary<string, IConfDefinition>();
      foreach (var configTransform in ConfBuilders) {
        configTransform.ApplyIn(configDefinitions);
      }
      return configDefinitions;
    }

    private void CreateScopedConfigDefinitions(IDictionary<string, IConfDefinition> configDefinitions) {
      var prefix = Scope + "/";
      var scopeDepth = prefix.Count(c => c == Keys.Separator);
      foreach (var configDefinition in CreateUnscopedConfigDefinitions()) {
        var newConfigDefinition = new PrefixedConfDefinition(prefix, scopeDepth, configDefinition.Value);
        var prefixedKey = prefix + configDefinition.Key;
        configDefinitions[prefixedKey] = newConfigDefinition;
      }
    }
  }
}