using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using Bud.Cli;
using Bud.Util;
using Bud.V1;
using Newtonsoft.Json;
using static Bud.V1.Api;

public class BudBuild : IBuild {
  private const int SampleCount = 7;

  public Conf Init()
    => Projects(CsApp("bud")
                  .SetValue(ProjectVersion, "0.5.0-pre-4")
                  .SetValue(ProjectUrl, "https://github.com/urbas/bud"),
                CsLib("Bud.Test")
                  .Add(Dependencies, "../bud"))
      .Init("benchmark", Benchmark);

  private static BenchmarkResults Benchmark(IConf c) {
    var benchmarksDir = Path.Combine(BaseDir[c], BuildDirName, "benchmarks");
    var cloneDir = Path.Combine(benchmarksDir, "bud-repo");
    if (Directory.Exists(benchmarksDir)) {
      Directory.Delete(benchmarksDir, true);
    }
    Directory.CreateDirectory(benchmarksDir);
    var budExe = CreateBudDistribution(c, benchmarksDir);
    const string revisionToBuild = "33e7262924b1eee21b521533c09d019d93a9b080";
    CopyRepo(BaseDir[c], cloneDir, revisionToBuild);
    var benchmark = new BenchmarkResults(
      Exec.GetOutput("git", "rev-parse HEAD"),
      "Mat's Lenovo Yoga 2 Pro",
      ImmutableList.Create(
//        MeasureColdBuildScriptLoad(budExe, cloneDir),
//        MeasureWarmBuildScriptLoad(budExe, cloneDir),
        MeasureColdProjectCompilation(budExe, cloneDir)));
    JsonSerializer.CreateDefault().Serialize(Console.Out, benchmark);
    // TODO: Benchmark 4: warm compile Bud.Test
    // TODO: Benchmark 5: warm compile Bud.Test with one source file touched
    // TODO: Benchmark 6: warm compile Bud.Test with one source file changed
    // TODO: Benchmark 7: warm compile Bud.Test with one source file in Bud touched
    // TODO: Benchmark 8: warm compile Bud.Test with one source file in Bud changed
    // TODO: Upload the zipped JSON containing benchmark results to BinTray.
    // TODO:   - PackageId: bud-benchmarks
    // TODO:   - vYYYY.M.D-bhhmmss
    // TODO: Take 7 samples of each measurement, discard the min and max, and take the average.
    // TODO: Do some nice visualisations with the average, error bars, and commit history.
    return null;
  }

  private static Measurement MeasureColdBuildScriptLoad(string budExe, string cloneDir) {
    var samples = ImmutableList.CreateBuilder<Sample>();
    for (int i = 0; i < SampleCount; i++) {
      Console.WriteLine($"Cold build script load run {i+1}/{SampleCount}");
      samples.Add(RunBud(budExe, cloneDir));
      ResetRepo(cloneDir);
    }
    return new Measurement("cold load build script", samples.ToImmutable());
  }

  private static Measurement MeasureWarmBuildScriptLoad(string budExe, string cloneDir) {
    var samples = ImmutableList.CreateBuilder<Sample>();
    RunBud(budExe, cloneDir);
    for (int i = 0; i < SampleCount; i++) {
      Console.WriteLine($"Warm build script load run {i+1}/{SampleCount}");
      samples.Add(RunBud(budExe, cloneDir));
    }
    ResetRepo(cloneDir);
    return new Measurement("warm load build script", samples.ToImmutable());
  }

  private static Measurement MeasureColdProjectCompilation(string budExe, string projectDir) {
    var samples = ImmutableList.CreateBuilder<Sample>();
    RunBud(budExe, projectDir);
    for (int i = 0; i < SampleCount; i++) {
      Console.WriteLine($"Cold Bud.Test compile run {i+1}/{SampleCount}");
      samples.Add(RunBud(budExe, projectDir, "Bud.Test/Compile"));
      RunBud(budExe, projectDir, "Bud/Clean Bud.Test/Clean");
    }
    return new Measurement("cold compile Bud.Test", samples.ToImmutable());
  }

  private static Sample RunBud(string budExe, string cloneDir, Option<string> buildCommand = default(Option<string>))
    => ToSample(Exec.RunCheckedQuietly(budExe, buildCommand.GetOrElse(""), cloneDir));

  private static Sample ToSample(Process executionInfo)
    => new Sample(executionInfo.TotalProcessorTime,
                  executionInfo.UserProcessorTime,
                  executionInfo.PrivilegedProcessorTime);

  /// <returns>
  ///   path to Bud's executable which is runnable as-is (all
  ///   required libraries are in the same folder).
  /// </returns>
  private static string CreateBudDistribution(IConf c, string benchmarksDir) {
    var filesToDist = c.Get("bud"/FilesToDistribute).Take(1).Wait();
    var budExeDir = Path.Combine(benchmarksDir, "bud-exe");
    foreach (var packageFile in filesToDist) {
      var path = Path.Combine(budExeDir, packageFile.PathInPackage);
      var dir = Path.GetDirectoryName(path);
      Directory.CreateDirectory(dir);
      File.Copy(packageFile.FileToPackage, path);
    }
    return Path.Combine(budExeDir, "bud");
  }

  private static void CopyRepo(string originalRepoDir, string cloneDir, string revision) {
    Exec.RunCheckedQuietly("git", $"clone -q -s -n {originalRepoDir} {cloneDir}");
    Exec.RunCheckedQuietly("git", $"-C {cloneDir} checkout -q --detach -f {revision}");
  }

  private static void ResetRepo(string repoDir) {
    Exec.RunCheckedQuietly("git", $"-C {repoDir} reset --hard HEAD");
    Exec.RunCheckedQuietly("git", $"-C {repoDir} clean -q -fdx .");
  }
}

public class BenchmarkResults {
  public BenchmarkResults(string vcsRevision, string machine, IImmutableList<Measurement> measurements) {
    VcsRevision = vcsRevision;
    Machine = machine;
    Measurements = measurements;
  }

  public string VcsRevision { get; }
  public string Machine { get; }
  public IImmutableList<Measurement> Measurements { get; }
}

public class Measurement {
  public Measurement(string id, IImmutableList<Sample> samples) {
    Id = id;
    Samples = samples;
  }

  public string Id { get; }
  public IImmutableList<Sample> Samples { get; }
}

public class Sample {
  public Sample(TimeSpan totalTime, TimeSpan userTime, TimeSpan systemTime) {
    TotalTime = totalTime;
    UserTime = userTime;
    SystemTime = systemTime;
  }

  public TimeSpan TotalTime { get; }
  public TimeSpan UserTime { get; }
  public TimeSpan SystemTime { get; }
}