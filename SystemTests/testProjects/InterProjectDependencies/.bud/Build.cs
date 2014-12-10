using Bud;
using Bud.Plugins.CSharp;
using Bud.Plugins.Dependencies;
using System.IO;

public class Build : IBuild {
  public Settings GetSettings(string baseDir) {
    var projectA = CSharp.LibraryProject("A", Path.Combine(baseDir, "A"));
    var projectB = CSharp.Project("B", Path.Combine(baseDir, "B")).DependsOn(projectA);
    return projectA.Add(projectB);
  }
}