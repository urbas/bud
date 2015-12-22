using System;
using System.Linq;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using static System.Reactive.Linq.Observable;
using static Bud.V1.Api;

namespace Bud.V1 {
  public class ApiDependenciesSupportTest {
    [Test]
    public void DependenciesInput_must_be_empty_when_no_dependencies_given() {
      var projects = BuildProject("aDir", "A");
      Assert.AreEqual(new[] {Enumerable.Empty<string>()},
                      projects.Get(DependenciesInput).ToList().Wait());
    }

    [Test]
    public void DependenciesInput_must_contain_output_from_dependencies() {
      var projects = Projects(BuildProject("aDir", "A")
                                .SetValue(Output, Return(new[] {"a"})),
                              BuildProject("bDir", "B")
                                .Add(Dependencies, "../A"));
      Assert.AreEqual(new[] {"a"},
                      projects.Get("B"/DependenciesInput).Wait());
    }


    [Test]
    public void DependenciesInput_reobserved_when_dependencies_change() {
      var testScheduler = new TestScheduler();
      var projects = Projects(BuildProject("aDir", "A")
                                .SetValue(BuildPipelineScheduler, testScheduler)
                                .SetValue(Output, ChangingOutput(testScheduler)),
                              BuildProject("bDir", "B")
                                .SetValue(BuildPipelineScheduler, testScheduler)
                                .Add(Dependencies, "../A"));
      var bInput = projects.Get("B"/DependenciesInput).GetEnumerator();
      testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
      Assert.IsTrue(bInput.MoveNext());
      Assert.AreEqual(new[] {"foo"}, bInput.Current);
      Assert.IsTrue(bInput.MoveNext());
      Assert.AreEqual(new[] {"bar"}, bInput.Current);
      Assert.IsFalse(bInput.MoveNext());
    }

    private static IObservable<string[]> ChangingOutput(TestScheduler testScheduler)
      => Return(new[] {"foo"}).Delay(TimeSpan.FromSeconds(1), testScheduler)
                              .Concat(Return(new[] {"bar"}).Delay(TimeSpan.FromSeconds(1), testScheduler));
  }
}