using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Bud.IO;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using static Bud.V1.Basic;
using static Bud.V1.Builds;
using static NUnit.Framework.Assert;

namespace Bud.V1 {
  public class BuildsTest {
    [Test]
    public void Input_should_be_initially_empty()
      => IsEmpty(BuildProject("A", "", "/foo").Get(Input).Take(1).Wait());

    [Test]
    public void DependenciesInput_must_be_empty_when_no_dependencies_given()
      => That(BuildProject("A", baseDir: "/foo").Get(DependenciesOutput).ToList().Wait(),
              Has.Exactly(1).Empty);

    [Test]
    public void DependenciesInput_must_contain_output_from_dependencies() {
      var projects = Projects(BuildProject("A", "foo")
                                      .Add(Output, "a"),
                                    BuildProject("B", "boo")
                                      .Add(Dependencies, "../A"));
      AreEqual(new[] {"a"},
               projects.Get("B"/DependenciesOutput).Wait());
    }


    [Test]
    public void DependenciesInput_reobserved_when_dependencies_change() {
      var testScheduler = new TestScheduler();
      var projects = Projects(BuildProject("A", "foo")
                                      .Set(BuildPipelineScheduler, testScheduler)
                                      .Set(Output, ChangingOutput(testScheduler)),
                                    BuildProject("B", "boo")
                                      .Set(BuildPipelineScheduler, testScheduler)
                                      .Add(Dependencies, "../A"));
      var bInput = projects.Get("B"/DependenciesOutput).GetEnumerator();
      testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
      IsTrue(bInput.MoveNext());
      AreEqual(new[] {"foo"}, bInput.Current);
      IsTrue(bInput.MoveNext());
      AreEqual(new[] {"bar"}, bInput.Current);
      IsFalse(bInput.MoveNext());
    }

    [Test]
    public void Input_should_initially_observe_a_single_empty_inout()
      => AreEqual(new[] {Enumerable.Empty<string>()},
                  BuildProject("A", "", "/foo").Get(Input).ToList().Wait());

    private static IObservable<IImmutableList<string>> ChangingOutput(IScheduler scheduler)
      => Observable.Return(new[] {"foo"}).Delay(TimeSpan.FromSeconds(1), scheduler)
                   .Concat(Observable.Return(new[] {"bar"})
                                     .Delay(TimeSpan.FromSeconds(1), scheduler))
      .Select(ImmutableList.ToImmutableList);

    private class FooAppenderInputProcessor : IInputProcessor {
      public IObservable<IEnumerable<string>> Process(IObservable<IEnumerable<string>> sources)
        => sources.Select(io => io.Select(file => file + "foo"));
    }

    public class ThreadIdRecordingInputProcessor : IInputProcessor {
      public int InvocationThreadId { get; private set; }

      public IObservable<IEnumerable<string>> Process(IObservable<IEnumerable<string>> sources) {
        InvocationThreadId = Thread.CurrentThread.ManagedThreadId;
        return sources;
      }
    }
  }
}