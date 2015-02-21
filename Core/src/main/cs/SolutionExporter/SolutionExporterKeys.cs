namespace Bud.SolutionExporter {
  public static class SolutionExporterKeys {
    public static readonly TaskKey ExportAsSolution = Key.Define("exportAsSolution", "Generates csproj files and a solution file for your C# projects in your build definition.");
  }
}