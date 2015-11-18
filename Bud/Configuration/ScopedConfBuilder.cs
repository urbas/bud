using System.Collections.Immutable;

namespace Bud.Configuration {
  public struct ScopedConfBuilder {
    public ScopedConfBuilder(ImmutableList<string> scope, IConfBuilder confBuilder) {
      Scope = scope;
      ConfBuilder = confBuilder;
    }

    public ImmutableList<string> Scope { get; }
    public IConfBuilder ConfBuilder { get; }
  }
}