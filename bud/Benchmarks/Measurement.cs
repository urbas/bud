using System;
using System.Collections.Immutable;
using System.Linq;
using Bud.Collections;
using Newtonsoft.Json;

namespace Bud.Benchmarks {
  public class Measurement {
    public Measurement(string id, ImmutableList<ImmutableDictionary<string, object>> samples) {
      Id = id;
      Samples = samples;
    }

    public string Id { get; }
    public ImmutableList<ImmutableDictionary<string, object>> Samples { get; }

    public static Measurement MeasureAndLog(string measurementId,
                                            int sampleCount,
                                            Func<int, ImmutableDictionary<string, object>> run,
                                            Action<int> preEachRun = null,
                                            Action<int> postEachRun = null,
                                            Action preRun = null,
                                            Action postRun = null)
      => Measure(measurementId,
                 sampleCount,
                 preRun: () => {
                   Console.WriteLine($"Measuring '{measurementId}':");
                   preRun?.Invoke();
                 },
                 preEachRun: i => {
                   Console.WriteLine($"Performing run {i}/{sampleCount}...");
                   preEachRun?.Invoke(i);
                 },
                 run: run,
                 postEachRun: postEachRun,
                 postRun: postRun);

    public static Measurement Measure(string measurementId,
                                      int sampleCount,
                                      Func<int, ImmutableDictionary<string, object>> run,
                                      Action<int> preEachRun = null,
                                      Action<int> postEachRun = null,
                                      Action preRun = null,
                                      Action postRun = null) {
      var samples = ImmutableList<ImmutableDictionary<string, object>>.Empty;
      preRun?.Invoke();
      for (int i = 1; i <= sampleCount; i++) {
        preEachRun?.Invoke(i);
        samples = samples.Add(run(i));
        postEachRun?.Invoke(i);
      }
      postRun?.Invoke();
      return new Measurement(measurementId, samples);
    }

    protected bool Equals(Measurement other)
      => string.Equals(Id, other.Id) &&
         other.Samples.Count == Samples.Count &&
         Samples.Zip(other.Samples, DictionaryUtils.DictionariesEqual)
                .All(b => b);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) {
        return false;
      }
      if (ReferenceEquals(this, obj)) {
        return true;
      }
      if (obj.GetType() != this.GetType()) {
        return false;
      }
      return Equals((Measurement) obj);
    }

    public override int GetHashCode() {
      unchecked {
        return (Id.GetHashCode()*397) ^ Samples.GetHashCode();
      }
    }

    public static bool operator ==(Measurement left, Measurement right) {
      return Equals(left, right);
    }

    public static bool operator !=(Measurement left, Measurement right) {
      return !Equals(left, right);
    }

    public override string ToString()
      => JsonConvert.SerializeObject(this, Formatting.Indented);
  }
}