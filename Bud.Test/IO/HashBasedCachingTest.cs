using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using static System.IO.Path;
using static NUnit.Framework.Assert;

namespace Bud.IO {
  public class HashBasedCachingTest {
    private TmpDir tmpDir;
    private string cacheFile;

    [SetUp]
    public void SetUp() {
      tmpDir = new TmpDir();
      cacheFile = Combine(tmpDir.Path, "some_lines");
    }

    [TearDown]
    public void TearDown() => tmpDir.Dispose();

    [Test]
    public void GetLinesOrCache_calculates_and_returns_the_lines()
      => AreEqual(GetSomeLines(),
                  HashBasedCaching.GetLinesOrCache(cacheFile, "a", GetSomeLines));

    [Test]
    public void GetLinesOrCache_does_not_calculate_on_the_second_call() {
      HashBasedCaching.GetLinesOrCache(cacheFile, "a", GetSomeLines);
      var lineCalculator = new Mock<Func<IEnumerable<string>>>(MockBehavior.Strict);
      AreEqual(GetSomeLines(),
               HashBasedCaching.GetLinesOrCache(cacheFile, "a", lineCalculator.Object));
    }

    [Test]
    public void GetLinesOrCache_calculates_when_the_digest_changes() {
      HashBasedCaching.GetLinesOrCache(cacheFile, "a", GetSomeLines);
      var lineCalculator = new Mock<Func<IEnumerable<string>>>(MockBehavior.Strict);
      lineCalculator.Setup(s => s()).Returns(GetSomeLines());
      AreEqual(GetSomeLines(),
               HashBasedCaching.GetLinesOrCache(cacheFile, "b", lineCalculator.Object));
      lineCalculator.VerifyAll();
    }

    private static IEnumerable<string> GetSomeLines() => new[] {"line 1", "line 2"};
  }
}