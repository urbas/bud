using System.Collections.Immutable;

namespace Bud.Benchmarks {
  public class Measurement {
    public Measurement(string id, IImmutableList<ISample> samples) {
      Id = id;
      Samples = samples;
    }

    public string Id { get; }
    public IImmutableList<ISample> Samples { get; }
  }
}