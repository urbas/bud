using Bud;
using Bud.Plugins.CSharp;
using Bud.Plugins.Dependencies;
using System.IO;

public class Build : IBuild {
  public Settings GetSettings(string baseDir) {
    var projectFoo = CSharp.Project("Foo", Path.Combine(baseDir, "Foo"));
    var projectBar = CSharp.Project("Bar", Path.Combine(baseDir, "Bar")).DependsOn(projectFoo);
    return projectFoo.Add(projectBar);
  }
}