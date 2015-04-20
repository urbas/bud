using Bud.CSharp;
using Bud.Projects;

namespace Bud.Examples.Snippets {
  public static class CSharpProjectSnippets {
    public static Settings LibraryProjectDefinition(Settings settings, string baseDir) {
      // SNIPPET: libraryProjectDefinition
      var libraryProject = new Project("Foo.Bar", baseDir, Cs.Dll());
      return settings.Add(libraryProject);
      // END_SNIPPET: libraryProjectDefinition
    }
  }
}