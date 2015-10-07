using System;
using System.Collections.Generic;
using System.Linq;
using Bud.Configuration;

namespace Bud {
  public static class ConfigExtensions {
    /// <param name="configs">the collection of configurations to which to add the transformation.</param>
    /// <param name="configTransform">an incremental modification of a configuration entry.</param>
    /// <returns>a copy of <paramref name="configs" /> with the added transformation.</returns>
    public static Configs Add(this Configs configs, IConfigTransform configTransform)
      => new Configs(configs.Concat(new[] {configTransform}));

    /// <summary>
    ///   Defines a constant-valued configuration.
    ///   If the configuration is already defined, then this method overwrites it.
    /// </summary>
    public static Configs Const<T>(this Configs configs, Key<T> configKey, T value)
      => configs.Set(configKey, cfg => value);

    /// <summary>
    ///   Defines a constant-valued configuration.
    ///   If the configuration is already defined, then this method does nothing.
    /// </summary>
    public static Configs InitConst<T>(this Configs configs, Key<T> configKey, T value)
      => configs.Init(configKey, cfg => value);

    /// <summary>
    ///   Defines a configuration that returns the value produced by <paramref name="valueFactory" />.
    ///   <paramref name="valueFactory" /> is invoked when the configuration is accessed.
    ///   If the configuration is already defined, then this method does nothing.
    /// </summary>
    public static Configs Init<TResult>(this Configs configs, Key<TResult> configKey, Func<IConfigs, TResult> valueFactory)
      => new Configs(configs.Add(new InitConfig<TResult>(configKey, valueFactory)));

    /// <summary>
    ///   Defines a configuration that returns the value produced by <paramref name="valueFactory" />.
    ///   <paramref name="valueFactory" /> is invoked when the configuration is accessed.
    ///   If the configuration is already defined, then this method overwrites the configuration.
    /// </summary>
    public static Configs Set<TResult>(this Configs configs, Key<TResult> configKey, Func<IConfigs, TResult> valueFactory)
      => new Configs(configs.Add(new SetConfig<TResult>(configKey, valueFactory)));

    /// <summary>
    ///   Defines a configuration that returns the value produced by <paramref name="valueFactory" />.
    ///   <paramref name="valueFactory" /> is invoked when the configuration is accessed.
    ///   If the configuration is already defined, then this method overwrites the configuration.
    /// </summary>
    public static Configs Modify<TResult>(this Configs configs, Key<TResult> configKey, Func<IConfigs, TResult, TResult> valueFactory)
      => new Configs(configs.Add(new ModifyConfig<TResult>(configKey, valueFactory)));

    /// <returns>a copy of <paramref name="configs" /> with added configurations from <paramref name="otherConfigs" />.</returns>
    public static Configs ExtendWith(this Configs configs, Configs otherConfigs)
      => new Configs(configs.Concat(otherConfigs));

    /// <returns>a copy of <paramref name="configs" /> with every configuration key prefixed with <paramref name="prefix"/>.</returns>
    public static Configs Nest(this Configs configs, string prefix)
      => new Configs(configs.Select(taskModification => new NestConfig(prefix, taskModification)));
    
    public static IDictionary<string, IConfigDefinition> Compile(this Configs configs) {
      var taskDefinitions = new Dictionary<string, IConfigDefinition>();
      foreach (var taskModification in configs) {
        IConfigDefinition configDefinition;
        if (taskDefinitions.TryGetValue(taskModification.Key, out configDefinition)) {
          taskDefinitions[taskModification.Key] = taskModification.Modify(configDefinition);
        } else {
          taskDefinitions.Add(taskModification.Key, taskModification.ToConfigDefinition());
        }
      }
      return taskDefinitions;
    }

    /// <summary>
    ///   Invokes the configuration and returns its value.
    /// </summary>
    /// <exception cref="ConfigTypeException">
    ///   thrown if the actual type of the configuration does not match the requested type <typeparamref name="T"/>.
    /// </exception>
    public static T Get<T>(this Configs configs, Key<T> configKey)
      => configs.ToCachingConfigs().Get(configKey);

    public static CachingConfigs ToCachingConfigs(this Configs configs)
      => new CachingConfigs(configs.Compile());
  }
}