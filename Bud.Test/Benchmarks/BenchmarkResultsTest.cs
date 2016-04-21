using System.Collections.Immutable;
using NUnit.Framework;

namespace Bud.Benchmarks {
  public class BenchmarkResultsTest {
    [Test]
    public void FromJson_deserializes_what_ToJson_serializes() {
      var sampleFoo = ImmutableDictionary<string, object>.Empty.Add("sam", 42L);
      var measurementFoo = new Measurement("foo", ImmutableList.Create(sampleFoo));
      var benchmarkResults = new BenchmarkResults("1234abcd", "bar", ImmutableArray.Create(measurementFoo));
      var deserializedBenchmarkResults = BenchmarkResults.FromJson(benchmarkResults.ToJson());
      Assert.AreEqual(benchmarkResults, deserializedBenchmarkResults);
    }
  }
}