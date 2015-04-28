using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.Logging;

namespace Bud {
  public interface IConfig {
    ImmutableDictionary<ConfigKey, IConfigDefinition> ConfigDefinitions { get; }

    ILogger Logger { get; }

    bool IsConfigDefined(Key key);

    Task Evaluate(Key key);

    T Evaluate<T>(ConfigKey<T> configKey);

    /// <remarks>
    ///   If the config is defined, this method will return <c>true</c> and <c>evaluatedValue</c> will contain
    ///   the value of the config.
    ///   <para>
    ///     If the config is not defined, this method will return <c>false</c> and <c>evaluatedValue</c> parameter will contain
    ///     the default value of type <typeparamref name="T" />.
    ///   </para>
    /// </remarks>
    bool TryEvaluate<T>(ConfigKey<T> configKey, out T evaluatedValue);

    object EvaluateConfig(Key key);
  }
}