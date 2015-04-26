using System;
using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Bud {
  public class Macro : Plugin {
    public static readonly ConfigKey<ImmutableDictionary<string, Macro>> Macros = Key.Define("macros");
    public const string MacroNamePrefix = "@";

    public Macro(string name, Func<Settings, object> function, string description = "")
      : this(name, (context, arguments) => new MacroResult(function(context.Settings), context), description) {}

    public Macro(string name, Func<BuildContext, object> function, string description = "")
      : this(name, (context, arguments) => new MacroResult(function(context), context), description) {}

    public Macro(string name, Func<Settings, Settings> function, string description = "")
      : this(name, (context, arguments) => new MacroResult(null, context.WithSettings(function(context.Settings))), description) {}

    public Macro(string name, Func<BuildContext, MacroResult> function, string description = "")
      : this(name, (context, arguments) => function(context), description) {}

    public Macro(string name, Func<Settings, string[], object> function, string description = "")
      : this(name, (context, arguments) => new MacroResult(function(context.Settings, arguments), context), description) {}

    public Macro(string name, Func<BuildContext, string[], object> function, string description = "")
      : this(name, (context, arguments) => new MacroResult(function(context, arguments), context), description) {}

    public Macro(string name, Func<Settings, string[], Settings> function, string description = "")
      : this(name, (context, arguments) => new MacroResult(null, context.WithSettings(function(context.Settings, arguments))), description) {}

    public Macro(string name, MacroFunction function, string description = "") {
      Name = name;
      Function = function;
      Description = description;
    }

    public string Name { get; }

    [JsonIgnore]
    public MacroFunction Function { get; }

    public string Description { get; }

    public override Settings Setup(Settings settings) {
      return settings.AddGlobally(Macros.Init(ImmutableDictionary<string, Macro>.Empty),
                                  Macros.Modify(macros => macros.Add(Name, this)));
    }
  }
}