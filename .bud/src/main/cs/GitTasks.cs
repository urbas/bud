using System;
using Bud.Cli;
using NuGet;

internal static class GitTasks {
  public static void GitTagRelease(SemanticVersion releaseVersion) {
    ProcessBuilder.Execute("git", "commit", "-am", String.Format("Release {0}", releaseVersion));
    ProcessBuilder.Execute("git", "tag", "v" + releaseVersion);
  }

  public static void GitCommitNextDevelopmentVersion(SemanticVersion nextDevelopmentVersion) {
    ProcessBuilder.Execute("git", "commit", "-am", String.Format("Setting next development version: {0}", nextDevelopmentVersion));
  }
}