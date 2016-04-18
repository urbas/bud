using System;
using System.Collections.Immutable;
using System.IO;
using System.Reactive.Linq;
using Bud.Benchmarks;
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
    benchmarkResults.ToJsonFile(Path.Combine(benchmarksDir, "benchmark-results.json"));
    var pushedUrl = benchmarkResults.PushToBintray("bud", "bud-benchmarks", "matej");
    if (!pushedUrl.HasValue) {
      throw new Exception("Failed to push benchmark results to bintray.");
    }
    // TODO: Add benchmarks for long chains of projects.
    // TODO: Add benchmarks for projects with loads of independent dependencies.
    // TODO: Do some nice visualisations with the average, error bars, and commit history.
    Console.WriteLine($"Pushed to {pushedUrl.Value}");
    return benchmarkResults;
  }

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
    var samples = ImmutableList.CreateBuilder<ISample>();
    for (int i = 0; i < SampleCount; i++) {
      Console.WriteLine($"{i + 1}/{SampleCount}");
      samples.Add(RunBud(budExe, cloneDir));
      ResetRepo(cloneDir);
    }
    return new Measurement("cold load build script", samples.ToImmutable());
  }

  private static Measurement MeasureWarmBuildScriptLoad(string budExe, string cloneDir) {
    Console.WriteLine("Warm build script load:");
    var samples = ImmutableList.CreateBuilder<ISample>();
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
    var samples = ImmutableList.CreateBuilder<ISample>();
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
    var samples = ImmutableList.CreateBuilder<ISample>();
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
    var samples = ImmutableList.CreateBuilder<ISample>();
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
    var samples = ImmutableList.CreateBuilder<ISample>();
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
    var samples = ImmutableList.CreateBuilder<ISample>();
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
    var samples = ImmutableList.CreateBuilder<ISample>();
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

  private static CpuTimeSample RunBud(string budExe, string cloneDir, Option<string> buildCommand = default(Option<string>))
    => CpuTimeSample.ToSample(Exec.RunCheckedQuietly(budExe, buildCommand.GetOrElse(""), cloneDir));

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