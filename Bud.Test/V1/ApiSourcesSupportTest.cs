using System.Reactive.Linq;
using Bud.IO;
using NUnit.Framework;
using static Bud.V1.Api;

namespace Bud.V1 {
  public class ApiSourcesSupportTest {
    [Test]
    public void Sources_should_be_initially_empty()
      => Assert.IsEmpty(Sources[BuildProject("bar", "Foo")].Take(1).Wait());

    [Test]
    public void Sources_should_contain_added_files() {
      var project = SourcesSupport.AddSourceFile("A")
                                  .AddSourceFile(_ => "B");
      Assert.That(Sources[project].Take(1).Wait(),
                  Is.EquivalentTo(new[] {"A", "B"}));
    }

    [Test]
    public void Sources_should_be_excluded_by_the_exclusion_filter() {
      var project = SourcesSupport.AddSourceFile("A")
                                  .AddSourceFile(_ => "B")
                                  .Add(SourceExcludeFilters, sourceFile => string.Equals("B", sourceFile));
      Assert.That(Sources[project].Take(1).Wait(),
                  Is.EquivalentTo(new[] {"A"}));
    }
  }
}