using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bud.Configuration;
using Bud.Util;

namespace Bud.V1 {
  public class Conf : IConfBuilder {
    public static Conf Empty { get; } = new Conf(ImmutableList<ScopedConfBuilder>.Empty, ImmutableList<string>.Empty);

    /// <summary>
    ///   The accumulated list of configurations. A value of a
    ///   configuration is obtained by applying configuration builders
    ///   one by one into a dictionary. Latter configuration builders
    ///   in this list can values created by configuration builders
    ///   that came before them.
    ///   <para>
    ///     Configuration builders come in three forms:
    ///   </para>
    ///   <para>
    ///     - initialisation (see <see cref="InitConf{T}" />), which initialises
    ///     the value of a configuration if the configuration has not yet been set,
    ///   </para>
    ///   <para>
    ///     - overridding (see <see cref="SetConf{T}" />), which sets the
    ///     value of a configuration regardless of whether the configuration was set, and
    ///   </para>
    ///   <para>
    ///     - modification (see <see cref="ModifyConf{T}" />), which uses the previous
    ///     value of a configuration to build the new value (useful for growing lists).
    ///   </para>
    /// </summary>
    private ImmutableList<ScopedConfBuilder> ScopedConfBuilders { get; }

    /// <summary>
    ///   The current directory is used when new key-value pairs are
    ///   added to this configuration. Keys will be interpreted as relative
    ///   paths against this directory.
    /// </summary>
    public ImmutableList<string> CurrentDir { get; }

    private Conf(ImmutableList<ScopedConfBuilder> scopedConfBuilders, ImmutableList<string> currentDir) {
      ScopedConfBuilders = scopedConfBuilders;
      CurrentDir = currentDir;
    }

    /// <summary>
    ///   Defines a constant-valued configuration.
    ///   If the configuration is already defined, then this method overwrites it.
    /// </summary>
    public Conf Set<T>(Key<T> configKey, T value)
      => Set(configKey, cfg => value);

    /// <summary>
    ///   Defines a constant-valued configuration.
    ///   If the configuration is already defined, then this method does nothing.
    /// </summary>
    public Conf Init<T>(Key<T> configKey, T value)
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
      => new Conf(ScopedConfBuilders.AddRange(ToScopedConfBuilders(otherConfs, CurrentDir)),
                  CurrentDir);

    /// <returns>
    ///   a copy of self where the <see cref="CurrentDir" /> is appended
    ///   with <paramref name="subDir" />.
    /// </returns>
    public Conf In(string subDir)
      => new Conf(ScopedConfBuilders, CurrentDir.Add(subDir));

    /// <returns>
    ///   a copy of self where the Scope has one element removed from the back.
    /// </returns>
    public Conf Out() {
      if (CurrentDir.IsEmpty) {
        throw new InvalidOperationException("Can not backtrack further. The Scope is already empty.");
      }
      return new Conf(ScopedConfBuilders, CurrentDir.RemoveAt(CurrentDir.Count - 1));
    }

    /// <param name="key">
    ///   The key for which to get the value. If the path of the key is relative,
    ///   it will be interpreted with the <see cref="CurrentDir" /> as the base path.
    /// </param>
    public Option<T> TryGet<T>(Key<T> key)
      => ToCompiled().TryGet<T>(Keys.InterpretFromScope(key, CurrentDir));

    public void AddTo(DirectoryDictionary<IConfDefinition> configDefinitions) {
      foreach (var scopedConfBuilder in ScopedConfBuilders) {
        scopedConfBuilder.ConfBuilder
                         .AddTo(configDefinitions.In(scopedConfBuilder.Scope));
      }
    }

    public IConf ToCompiled() {
      var definitions = new Dictionary<string, IConfDefinition>();
      AddTo(new DirectoryDictionary<IConfDefinition>(definitions, ImmutableList<string>.Empty));
      return new RawConf(definitions);
    }

    public static Conf Group(string scope, params IConfBuilder[] confs) {
      var scopeAsList = ImmutableList.Create(scope);
      return new Conf(ToScopedConfBuilders(confs, scopeAsList).ToImmutableList(),
                      scopeAsList);
    }

    public static Conf Group(params IConfBuilder[] confs)
      => Group((IEnumerable<IConfBuilder>) confs);

    public static Conf Group(IEnumerable<IConfBuilder> confs)
      => Empty.Add(confs);

    internal static IEnumerable<ScopedConfBuilder> ToScopedConfBuilders(IEnumerable<IConfBuilder> otherConfs, ImmutableList<string> scope)
      => otherConfs.Select(builder => new ScopedConfBuilder(scope, builder));
  }
}