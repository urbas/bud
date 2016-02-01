using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using static System.IO.Path;
using static NUnit.Framework.Assert;

namespace Bud.IO {
  public class HashBasedCachingTest {
    private TemporaryDirectory tmpDir;
    private string cacheFile;

    [SetUp]
    public void SetUp() {
      tmpDir = new TemporaryDirectory();
      cacheFile = Combine(tmpDir.Path, "some_lines");
    }

    [TearDown]
    public void TearDown() => tmpDir.Dispose();

    [Test]
    public void TryGetLines_calculates_and_returns_the_lines()
      => AreEqual(GetSomeLines(),
                  HashBasedCaching.Get(cacheFile, "a", GetSomeLines));

    [Test]
    public void TryGetLines_does_not_calculate_on_the_second_call() {
      HashBasedCaching.Get(cacheFile, "a", GetSomeLines);
      var lineCalculator = new Mock<Func<IEnumerable<string>>>(MockBehavior.Strict);
      AreEqual(GetSomeLines(),
               HashBasedCaching.Get(cacheFile, "a", lineCalculator.Object));
    }

    [Test]
    public void TryGetLines_calculates_when_the_digest_changes() {
      HashBasedCaching.Get(cacheFile, "a", GetSomeLines);
      var lineCalculator = new Mock<Func<IEnumerable<string>>>(MockBehavior.Strict);
      lineCalculator.Setup(s => s()).Returns(GetSomeLines());
      AreEqual(GetSomeLines(),
               HashBasedCaching.Get(cacheFile, "b", lineCalculator.Object));
      lineCalculator.VerifyAll();
    }

    private static IEnumerable<string> GetSomeLines() => new[] {"line 1", "line 2"};
  }
}