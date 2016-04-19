using NUnit.Framework;
using static Bud.Benchmarks.Measurement;

namespace Bud.Benchmarks {
  public class MeasurementTest {
    [Test]
    public void Take_returns_a_valid_measurement() {
      var number = 0;
      var expected = new Measurement(
        "foo",
        Samples.None
               .Add(Samples.Empty.Add("bar", 43))
               .Add(Samples.Empty.Add("bar", 46)));

      var measurement = Measure(
        "foo",
        sampleCount: 2,
        run: i => Samples.Empty.Add("bar", number),
        preEachRun: i => number = number + i,
        postEachRun: i => number++,
        preRun: () => number = 42,
        postRun: () => number = -1);

      Assert.AreEqual(number, -1);
      Assert.AreEqual(expected, measurement);
    }
  }
}