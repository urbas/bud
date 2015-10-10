using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bud.Configuration;

namespace Bud {
  public struct Configs : IEnumerable<IConfigTransform>, IConfigs {
    public static Configs Empty { get; } = new Configs(Enumerable.Empty<IConfigTransform>());
    private IEnumerable<IConfigTransform> ConfigTransforms { get; }

    public Configs(IEnumerable<IConfigTransform> configTransforms) {
      ConfigTransforms = configTransforms;
    }

    public IEnumerator<IConfigTransform> GetEnumerator() => ConfigTransforms.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) ConfigTransforms).GetEnumerator();

    /// <param name="configTransform">an incremental modification of a configuration entry.</param>
    /// <returns>a copy of self with the added transformation.</returns>
    public Configs Add(IConfigTransform configTransform)
      => new Configs(this.Concat(new[] {configTransform}));

    /// <summary>
    ///   Defines a constant-valued configuration.
    ///   If the configuration is already defined, then this method overwrites it.
    /// </summary>
    public Configs Const<T>(Key<T> configKey, T value) => Set(configKey, cfg => value);

    /// <summary>
    ///   Defines a constant-valued configuration.
    ///   If the configuration is already defined, then this method does nothing.
    /// </summary>
    public Configs InitConst<T>(Key<T> configKey, T value) => Init(configKey, cfg => value);

    /// <summary>
    ///   Defines a configuration that returns the value produced by <paramref name="valueFactory" />.
    ///   <paramref name="valueFactory" /> is invoked when the configuration is accessed.
    ///   If the configuration is already defined, then this method does nothing.
    /// </summary>
    public Configs Init<T>(Key<T> configKey, Func<IConfigs, T> valueFactory)
      => Add(new InitConfig<T>(configKey, valueFactory));

    /// <summary>
    ///   Defines a configuration that returns the value produced by <paramref name="valueFactory" />.
    ///   <paramref name="valueFactory" /> is invoked when the configuration is accessed.
    ///   If the configuration is already defined, then this method overwrites the configuration.
    /// </summary>
    public Configs Set<T>(Key<T> configKey, Func<IConfigs, T> valueFactory)
      => Add(new SetConfig<T>(configKey, valueFactory));

    /// <summary>
    ///   Defines a configuration that returns the value produced by <paramref name="valueFactory" />.
    ///   <paramref name="valueFactory" /> is invoked when the configuration is accessed.
    ///   If the configuration is already defined, then this method overwrites the configuration.
    /// </summary>
    public Configs Modify<T>(Key<T> configKey, Func<IConfigs, T, T> valueFactory)
      => Add(new ModifyConfig<T>(configKey, valueFactory));

    /// <returns>a copy of self with added configurations from <paramref name="otherConfigs" />.</returns>
    public Configs Add(params Configs[] otherConfigs)
      => otherConfigs.Aggregate(this, (configs, configs1) => new Configs(configs.Concat(configs1)));

    /// <returns>a copy of self with every configuration key prefixed with <paramref name="parentKey" />.</returns>
    public Configs Nest(string parentKey)
      => new Configs(this.Select(configTransform => configTransform.Nest(parentKey)));

    public IDictionary<string, IConfigDefinition> Bake() {
      var configDefinitions = new Dictionary<string, IConfigDefinition>();
      foreach (var configTransform in this) {
        IConfigDefinition configDefinition;
        if (configDefinitions.TryGetValue(configTransform.Key, out configDefinition)) {
          configDefinitions[configTransform.Key] = configTransform.Modify(configDefinition);
        } else {
          configDefinitions.Add(configTransform.Key, configTransform.ToConfigDefinition());
        }
      }
      return configDefinitions;
    }

    /// <summary>
    ///   Invokes the configuration and returns its value.
    /// </summary>
    /// <exception cref="ConfigTypeException">
    ///   thrown if the actual type of the configuration does not match the requested type <typeparamref name="T" />.
    /// </exception>
    public T Get<T>(Key<T> configKey) => ToCachingConfigs().Get(configKey);

    public CachingConfigs ToCachingConfigs() => new CachingConfigs(Bake());
  }
}