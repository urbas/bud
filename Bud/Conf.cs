using System;
using System.Collections.Generic;
using System.Linq;
using Bud.Configuration;

namespace Bud {
  public struct Conf : IConf, IConfigTransform {
    public static Conf Empty { get; } = new Conf(Enumerable.Empty<IConfigTransform>(), string.Empty);
    private IEnumerable<IConfigTransform> ConfigTransforms { get; }
    public string Scope { get; }

    public Conf(IEnumerable<IConfigTransform> configTransforms,
                string scope) {
      ConfigTransforms = configTransforms;
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
      => Add(new InitConfig<T>(configKey, valueFactory));

    /// <summary>
    ///   Defines a configuration that returns the value produced by <paramref name="valueFactory" />.
    ///   <paramref name="valueFactory" /> is invoked when the configuration is accessed.
    ///   If the configuration is already defined, then this method overwrites the configuration.
    /// </summary>
    public Conf Set<T>(Key<T> configKey, Func<IConf, T> valueFactory)
      => Add(new SetConfig<T>(configKey, valueFactory));

    /// <summary>
    ///   2
    ///   Defines a configuration that returns the value produced by <paramref name="valueFactory" />.
    ///   <paramref name="valueFactory" /> is invoked when the configuration is accessed.
    ///   If the configuration is already defined, then this method overwrites the configuration.
    /// </summary>
    public Conf Modify<T>(Key<T> configKey, Func<IConf, T, T> valueFactory)
      => Add(new ModifyConfig<T>(configKey, valueFactory));

    /// <returns>a copy of self with added configurations from <paramref name="otherConfs" />.</returns>
    public Conf Add(params IConfigTransform[] otherConfs)
      => Add((IEnumerable<IConfigTransform>) otherConfs);

    public Conf Add(IEnumerable<IConfigTransform> otherConfs)
      => new Conf(ConfigTransforms.Concat(otherConfs), Scope);

    public static Conf Group(params IConfigTransform[] configTransforms)
      => Empty.Add(configTransforms);

    public static Conf Group(IEnumerable<IConfigTransform> configTransforms)
      => Empty.Add(configTransforms);

    public Conf Add<T>(Key<IEnumerable<T>> dependencies, params T[] v)
      => Modify(dependencies, (conf, enumerable) => enumerable.Concat(v));

    /// <returns>a copy of self where every configuration key is prefixed with <paramref name="scope" />.</returns>
    public Conf In(string scope)
      => new Conf(ConfigTransforms, Keys.PrefixWith(scope, Scope));

    /// <returns>the value of the configuration key.</returns>
    /// <exception cref="ConfigTypeException">
    ///   thrown if the actual type of the configuration does not match the requested type <typeparamref name="T" />.
    /// </exception>
    public T Get<T>(Key<T> configKey)
      => ToCachingConf().Get(configKey);

    public void ApplyIn(IDictionary<string, IConfigDefinition> configDefinitions) {
      if (string.IsNullOrEmpty(Scope)) {
        CreateUnscopedConfigDefinitions(configDefinitions);
      } else {
        CreateScopedConfigDefinitions(configDefinitions);
      }
    }

    public CachingConf ToCachingConf()
      => new CachingConf(CreateUnscopedConfigDefinitions());

    private IDictionary<string, IConfigDefinition> CreateUnscopedConfigDefinitions(IDictionary<string, IConfigDefinition> configDefinitions = null) {
      configDefinitions = configDefinitions ?? new Dictionary<string, IConfigDefinition>();
      foreach (var configTransform in ConfigTransforms) {
        configTransform.ApplyIn(configDefinitions);
      }
      return configDefinitions;
    }

    private void CreateScopedConfigDefinitions(IDictionary<string, IConfigDefinition> configDefinitions) {
      var prefix = Scope + "/";
      foreach (var configDefinition in CreateUnscopedConfigDefinitions()) {
        var newConfigDefinition = new PrefixedConfigDefinition(prefix, configDefinition.Value);
        var prefixedKey = prefix + configDefinition.Key;
        configDefinitions[prefixedKey] = newConfigDefinition;
      }
    }
  }
}