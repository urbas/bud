using System.Reactive.Linq;
using NUnit.Framework;

namespace Bud.IO {
  public class FilesTest {
    [Test]
    public void Empty_enumerates_no_files() => Assert.IsEmpty(Files.Empty.Enumerate());

    [Test]
    public void Empty_observes_only_once()
      => Assert.That(Files.Empty.Watch().ToEnumerable(),
                     Is.EquivalentTo(new[] {new FilesUpdate(null, Files.Empty)}));
    [Test]
    public void Empty_observable_is_reusable() {
      Empty_observes_only_once();
      Empty_observes_only_once();
    }

    [Test]
    public void Empty_enumeration_is_reusable() {
      Empty_enumerates_no_files();
      Empty_enumerates_no_files();
    }
  }
}