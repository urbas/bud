using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using Bud.Cli;
using Bud.Dist;
using Bud.Util;
using Bud.V1;
using Newtonsoft.Json;
using static Bud.V1.Api;

public class BudBuild : IBuild {
  public Conf Init()
    => Projects(CsApp("bud")
                  .SetValue(ProjectVersion, "0.5.0-pre-5")
                  .SetValue(ProjectUrl, "https://github.com/urbas/bud"),
                CsLib("Bud.Test")
                  .Add(Dependencies, "../bud"))
      .Init("benchmark", BudBenchmarks.Benchmark);
}

internal static class BudBenchmarks {
  private const int SampleCount = 7;

  internal static BenchmarkResults Benchmark(IConf c) {
    var benchmarksDir = CreateBenchmarksDir(c);
    var cloneDir = CloneProjectToBuild(c, benchmarksDir);
    var budExe = CreateBudExe(c, benchmarksDir);
    var revisionBeingBenchmarked = Exec.GetOutput("git", "rev-parse HEAD");
    var benchmarkResults = new BenchmarkResults(revisionBeingBenchmarked,
                                                Environment.MachineName,
                                                TakeMeasurements(budExe, cloneDir));
    var pushedUrl = PushBenchmarkResultsToBinTray(benchmarksDir,
                                                  benchmarkResults,
                                                  revisionBeingBenchmarked);
    if (!pushedUrl.HasValue) {
      throw new Exception("Failed to push benchmark results to bintray.");
    }
    // TODO: Add benchmarks for long chains of projects.
    // TODO: Add benchmarks for projects with loads of independent dependencies.
    // TODO: Do some nice visualisations with the average, error bars, and commit history.
    Console.WriteLine($"Pushed to {pushedUrl.Value}");
    return benchmarkResults;
  }

  private static Option<string> PushBenchmarkResultsToBinTray(string benchmarksDir, BenchmarkResults benchmarkResults, string revisionToBenchmark)
    => BinTrayDistribution.PushToBintray(
      CreateBenchmarkResultsFile(benchmarksDir, benchmarkResults),
      "bud",
      "bud-benchmarks",
      $"{DateTime.Now.ToString("yyyy.M.d-bHHmmss")}-{revisionToBenchmark.Substring(0, 8)}",
      "matej",
      "json");

  private static string CloneProjectToBuild(IConf c, string benchmarksDir) {
    var cloneDir = Path.Combine(benchmarksDir, "bud-repo");
    const string revisionToBuild = "33e7262924b1eee21b521533c09d019d93a9b080";
    CopyRepo(BaseDir[c], cloneDir, revisionToBuild);
    return cloneDir;
  }

  private static string CreateBenchmarksDir(IConf c) {
    var benchmarksDir = Path.Combine(BaseDir[c], BuildDirName, "benchmarks");
    if (Directory.Exists(benchmarksDir)) {
      Directory.Delete(benchmarksDir, true);
    }
    Directory.CreateDirectory(benchmarksDir);
    return benchmarksDir;
  }

  private static string CreateBenchmarkResultsFile(string benchmarksDir, BenchmarkResults benchmarkResults) {
    var benchmarkResultsFile = Path.Combine(benchmarksDir, "benchmark-results.json");
    using (var fileWriter = File.Open(benchmarkResultsFile, FileMode.Create, FileAccess.Write)) {
      using (var textFileWriter = new StreamWriter(fileWriter)) {
        JsonSerializer
          .CreateDefault(new JsonSerializerSettings {Formatting = Formatting.Indented})
          .Serialize(textFileWriter, benchmarkResults);
      }
    }
    return benchmarkResultsFile;
  }

  private static ImmutableList<Measurement> TakeMeasurements(string budExe, string cloneDir)
    => ImmutableList.Create(
      MeasureColdBuildScriptLoad(budExe, cloneDir),
      MeasureWarmBuildScriptLoad(budExe, cloneDir),
      MeasureColdProjectCompilation(budExe, cloneDir),
      MeasureWarmProjectCompilation(budExe, cloneDir),
      MeasureWarmProjectCompilationOneFileTouched(budExe, cloneDir),
      MeasureWarmProjectCompilationOneDependentFileTouched(budExe, cloneDir),
      MeasureWarmProjectCompilationOneFileChanged(budExe, cloneDir),
      MeasureWarmProjectCompilationOneDependentFileChanged(budExe, cloneDir));

  private static Measurement MeasureColdBuildScriptLoad(string budExe, string cloneDir) {
    Console.WriteLine("Cold build script load:");
    var samples = ImmutableList.CreateBuilder<Sample>();
    for (int i = 0; i < SampleCount; i++) {
      Console.WriteLine($"{i + 1}/{SampleCount}");
      samples.Add(RunBud(budExe, cloneDir));
      ResetRepo(cloneDir);
    }
    return new Measurement("cold load build script", samples.ToImmutable());
  }

  private static Measurement MeasureWarmBuildScriptLoad(string budExe, string cloneDir) {
    Console.WriteLine("Warm build script load:");
    var samples = ImmutableList.CreateBuilder<Sample>();
    RunBud(budExe, cloneDir);
    for (int i = 0; i < SampleCount; i++) {
      Console.WriteLine($"{i + 1}/{SampleCount}");
      samples.Add(RunBud(budExe, cloneDir));
    }
    ResetRepo(cloneDir);
    return new Measurement("warm load build script", samples.ToImmutable());
  }

  private static Measurement MeasureColdProjectCompilation(string budExe, string projectDir) {
    Console.WriteLine("Cold Bud.Test compile:");
    var samples = ImmutableList.CreateBuilder<Sample>();
    RunBud(budExe, projectDir);
    for (int i = 0; i < SampleCount; i++) {
      Console.WriteLine($"{i + 1}/{SampleCount}");
      samples.Add(RunBud(budExe, projectDir, "Bud.Test/Compile"));
      RunBud(budExe, projectDir, "Bud/Clean Bud.Test/Clean");
    }
    ResetRepo(projectDir);
    return new Measurement("cold compile Bud.Test", samples.ToImmutable());
  }

  private static Measurement MeasureWarmProjectCompilation(string budExe, string projectDir) {
    Console.WriteLine("Warm Bud.Test compile:");
    var samples = ImmutableList.CreateBuilder<Sample>();
    RunBud(budExe, projectDir);
    RunBud(budExe, projectDir, "Bud.Test/Compile");
    for (int i = 0; i < SampleCount; i++) {
      Console.WriteLine($"{i + 1}/{SampleCount}");
      samples.Add(RunBud(budExe, projectDir, "Bud.Test/Compile"));
    }
    ResetRepo(projectDir);
    return new Measurement("warm compile Bud.Test", samples.ToImmutable());
  }

  private static Measurement MeasureWarmProjectCompilationOneFileTouched(string budExe, string projectDir) {
    var fileToTouch = Path.Combine(projectDir, "Bud.Test", "V1", "KeysTest.cs");
    Console.WriteLine($"Warm Bud.Test compile (touched file '{fileToTouch}'):");
    var samples = ImmutableList.CreateBuilder<Sample>();
    RunBud(budExe, projectDir);
    RunBud(budExe, projectDir, "Bud.Test/Compile");
    for (int i = 0; i < SampleCount; i++) {
      File.SetLastWriteTimeUtc(fileToTouch, DateTime.UtcNow);
      Console.WriteLine($"{i + 1}/{SampleCount}");
      samples.Add(RunBud(budExe, projectDir, "Bud.Test/Compile"));
    }
    ResetRepo(projectDir);
    return new Measurement("warm compile Bud.Test, touched a Bud.Test source file", samples.ToImmutable());
  }

  private static Measurement MeasureWarmProjectCompilationOneFileChanged(string budExe, string projectDir) {
    var fileToChange = Path.Combine(projectDir, "Bud.Test", "V1", "KeysTest.cs");
    Console.WriteLine($"Warm Bud.Test compile (changed file '{fileToChange}'):");
    var samples = ImmutableList.CreateBuilder<Sample>();
    RunBud(budExe, projectDir);
    RunBud(budExe, projectDir, "Bud.Test/Compile");
    for (int i = 0; i < SampleCount; i++) {
      File.AppendAllText(fileToChange, "\n// Just a comment\n");
      Console.WriteLine($"{i + 1}/{SampleCount}");
      samples.Add(RunBud(budExe, projectDir, "Bud.Test/Compile"));
    }
    ResetRepo(projectDir);
    return new Measurement("warm compile Bud.Test, changed a Bud.Test source file", samples.ToImmutable());
  }

  private static Measurement MeasureWarmProjectCompilationOneDependentFileTouched(string budExe, string projectDir) {
    var fileToTouch = Path.Combine(projectDir, "bud", "V1", "Keys.cs");
    Console.WriteLine($"Warm Bud.Test compile (touched file '{fileToTouch}'):");
    var samples = ImmutableList.CreateBuilder<Sample>();
    RunBud(budExe, projectDir);
    RunBud(budExe, projectDir, "Bud.Test/Compile");
    for (int i = 0; i < SampleCount; i++) {
      File.SetLastWriteTimeUtc(fileToTouch, DateTime.UtcNow);
      Console.WriteLine($"{i + 1}/{SampleCount}");
      samples.Add(RunBud(budExe, projectDir, "Bud.Test/Compile"));
    }
    ResetRepo(projectDir);
    return new Measurement("warm compile Bud.Test, touched a bud source file", samples.ToImmutable());
  }

  private static Measurement MeasureWarmProjectCompilationOneDependentFileChanged(string budExe, string projectDir) {
    var fileToChange = Path.Combine(projectDir, "bud", "V1", "Keys.cs");
    Console.WriteLine($"Warm Bud.Test compile (changed file '{fileToChange}'):");
    var samples = ImmutableList.CreateBuilder<Sample>();
    RunBud(budExe, projectDir);
    RunBud(budExe, projectDir, "Bud.Test/Compile");
    for (int i = 0; i < SampleCount; i++) {
      File.AppendAllText(fileToChange, "\n// Just a comment\n");
      Console.WriteLine($"{i + 1}/{SampleCount}");
      samples.Add(RunBud(budExe, projectDir, "Bud.Test/Compile"));
    }
    ResetRepo(projectDir);
    return new Measurement("warm compile Bud.Test, changed a bud source file", samples.ToImmutable());
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
  private static string CreateBudExe(IConf c, string benchmarksDir) {
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