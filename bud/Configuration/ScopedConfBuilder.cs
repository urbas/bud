using System.Collections.Immutable;

namespace Bud.Configuration {
  internal struct ScopedConfBuilder {
    public ScopedConfBuilder(ImmutableList<string> dir, IConfBuilder confBuilder) {
      Dir = dir;
      ConfBuilder = confBuilder;
    }

    public ImmutableList<string> Dir { get; }
    public IConfBuilder ConfBuilder { get; }
  }
}