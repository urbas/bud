using System.Collections.Immutable;

namespace Bud.Configuration {
  internal struct ScopedConfBuilder {
    public ScopedConfBuilder(IImmutableList<string> dir, IConfBuilder confBuilder) {
      Dir = dir;
      ConfBuilder = confBuilder;
    }

    public IImmutableList<string> Dir { get; }
    public IConfBuilder ConfBuilder { get; }
  }
}