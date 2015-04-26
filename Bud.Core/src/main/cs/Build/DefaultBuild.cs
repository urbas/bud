using System.IO;
using Bud.CSharp;
using Bud.Projects;

namespace Bud.Build {
  public class DefaultBuild : IBuild {
    public Settings Setup(Settings settings, string dirOfProjectToBeBuilt) {
      var project = new Project(Path.GetFileName(dirOfProjectToBeBuilt), dirOfProjectToBeBuilt, Cs.Exe(), Cs.Test());
      return settings.Add(project);
    }
  }
}