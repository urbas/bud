using CommandLine;
using CommandLine.Text;

public class PerformReleaseArguments {
  [Option('v', "version", Required = true, HelpText = "The version to release.")]
  public string Version { get; set; }

  [Option('d', "dev-version", HelpText = "Next development version. If omitted, the release version with an incremented patch component is used.")]
  public string NextDevelopmentVersion { get; set; }

  [HelpOption]
  public string GetUsage() {
    var helpText = HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
    helpText.Heading = new HeadingInfo("Bud: perform release macro");
    helpText.Copyright = new CopyrightInfo("Matej Urbas", 2015);
    helpText.AddPreOptionsLine("\nUsage: bud @performRelease <options>");
    helpText.AddPreOptionsLine("\nOptions:");
    return helpText;
  }
}