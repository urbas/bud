using Bud;
using Bud.CSharp;
using Bud.Projects;
using Bud.Examples.HelloWorldPlugin;

public class Build : IBuild {
  public Settings Setup(Settings settings, string baseDir) {
    return settings.Add(new HelloWorldPlugin(),
                        new Project("A", baseDir, Cs.Exe()));
  }
}