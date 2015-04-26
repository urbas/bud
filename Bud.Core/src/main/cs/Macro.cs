using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Bud {
  public delegate Settings MacroFunction(Settings settings, string[] commandLineArguments);

  public class Macro : Plugin {
    public static readonly ConfigKey<ImmutableDictionary<string, Macro>> Macros = Key.Define("macros");
    public const string MacroNamePrefix = "@";

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