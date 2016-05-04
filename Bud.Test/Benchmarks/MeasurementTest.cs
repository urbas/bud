using NUnit.Framework;
using static Bud.Benchmarks.Measurement;
using static Bud.Benchmarks.Samples;

namespace Bud.Benchmarks {
  public class MeasurementTest {
    [Test]
    public void Measure_invokes_the_run_callback_twice() {
      var runs = 0;
      Measure(measurementId: "foo",
              sampleCount: 2,
              run: i => {
                runs++;
                return DataPoint("bar", i);
              });
      Assert.AreEqual(2, runs);
    }

    [Test]
    public void Measure_invokes_the_run_callback_with_indices_starting_with_1() {
      var expectedSamples = SampleList(DataPoint("bar", 1),
                                       DataPoint("bar", 2));
      var expectedMeasurement = new Measurement("foo", expectedSamples);
      var measurement = Measure(measurementId: "foo",
                                sampleCount: 2,
                                run: i => DataPoint("bar", i));
      Assert.AreEqual(expectedMeasurement, measurement);
    }

    [Test]
    public void Measure_invokes_the_preRun_callback_once_before_running() {
      int number = 0;
      Measure(measurementId: "foo",
              sampleCount: 2,
              preRun: () => ++number,
              run: i => {
                Assert.AreEqual(1, number);
                return DataPoint("bar", i);
              });
    }

    [Test]
    public void Measure_invokes_the_preEachRun_callback_before_each_run() {
      int number = 0;
      Measure(measurementId: "foo",
              sampleCount: 2,
              preRun: () => Assert.AreEqual(0, number),
              preEachRun: i => number = i,
              run: i => {
                Assert.AreEqual(i, number);
                return DataPoint("bar", i);
              });
    }

    [Test]
    public void Measure_invokes_the_postEachRun_callback_after_each_run() {
      int number = 0;
      Measure(measurementId: "foo",
              sampleCount: 2,
              run: i => {
                Assert.AreEqual(i - 1, number);
                return DataPoint("bar", i);
              },
              postEachRun: i => number = i,
              postRun: () => Assert.AreEqual(2, number));
    }

    [Test]
    public void Measure_invokes_the_postRun_callback_at_the_end() {
      int number = 0;
      Measure(measurementId: "foo",
              sampleCount: 2,
              run: i => {
                Assert.AreEqual(0, number);
                return DataPoint("bar", i);
              },
              postEachRun: i => Assert.AreEqual(0, number),
              postRun: () => ++number);
      Assert.AreEqual(1, number);
    }
  }
}