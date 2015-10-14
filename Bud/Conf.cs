using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bud.Configuration;

namespace Bud {
  public struct Conf : IEnumerable<IConfigTransform>, IConf {
    public static Conf Empty { get; } = new Conf(Enumerable.Empty<IConfigTransform>());
    private IEnumerable<IConfigTransform> ConfigTransforms { get; }

    public Conf(IEnumerable<IConfigTransform> configTransforms) {
      ConfigTransforms = configTransforms;
    }

    public IEnumerator<IConfigTransform> GetEnumerator() => ConfigTransforms.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) ConfigTransforms).GetEnumerator();

    /// <param name="configTransform">an incremental modification of a configuration entry.</param>
    /// <returns>a copy of self with the added transformation.</returns>
    public Conf Add(IConfigTransform configTransform)
      => new Conf(this.Concat(new[] {configTransform}));

    /// <summary>
    ///   Defines a constant-valued configuration.
    ///   If the configuration is already defined, then this method overwrites it.
    /// </summary>
    public Conf Const<T>(Key<T> configKey, T value) => Set(configKey, cfg => value);

    /// <summary>
    ///   Defines a constant-valued configuration.
    ///   If the configuration is already defined, then this method does nothing.
    /// </summary>
    public Conf InitConst<T>(Key<T> configKey, T value) => Init(configKey, cfg => value);

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
    ///   Defines a configuration that returns the value produced by <paramref name="valueFactory" />.
    ///   <paramref name="valueFactory" /> is invoked when the configuration is accessed.
    ///   If the configuration is already defined, then this method overwrites the configuration.
    /// </summary>
    public Conf Modify<T>(Key<T> configKey, Func<IConf, T, T> valueFactory)
      => Add(new ModifyConfig<T>(configKey, valueFactory));

    /// <returns>a copy of self with added configurations from <paramref name="otherConf" />.</returns>
    public Conf Add(params Conf[] otherConf)
      => otherConf.Aggregate(this, (configs, configs1) => new Conf(configs.Concat(configs1)));

    /// <returns>a copy of self with every configuration key prefixed with <paramref name="prefix" />.</returns>
    public static Conf operator /(string prefix, Conf conf)
      => new Conf(conf.Select(configTransform => configTransform.Nest(prefix)));

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

    public CachingConf ToCachingConfigs() => new CachingConf(Bake());
  }
}