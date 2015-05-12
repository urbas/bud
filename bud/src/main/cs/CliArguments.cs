using System.Collections.Generic;
using Bud.Info;
using CommandLine;
using CommandLine.Text;

namespace Bud {
  public class CliArguments {
    [Option('b', "build-level", DefaultValue = 0, HelpText = "The build configuration level at which to evaluate tasks. Level 0 corresponds to the build configuration defined in '.bud/Build.cs', level 1 to '.bud/.bud/Build.cs' and so on.")]
    public int BuildLevel { get; set; }

    [Option('j', "json", HelpText = "Prints the value of the evaluated task as JSON to the standard output.")]
    public bool PrintJson { get; set; }

    [Option('q', "quiet", HelpText = "Suppresses logs.")]
    public bool IsQuiet { get; set; }

    [Option('s', "enable-stack-traces", HelpText = "Prints exception stack traces.")]
    public bool AreStackTracesEnabled { get; set; }

    [Option('v', "version", HelpText = "Displays Bud's version and exits immediately after.")]
    public bool IsShowVersion { get; set; }

    [ValueList(typeof(List<string>))]
    public List<string> Commands { get; set; }

    [HelpOption]
    public string GetUsage() {
      var helpText = HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
      helpText.Heading = new HeadingInfo("Bud: the build tool without the 'ill'.", "v" + BudVersion.Current);
      helpText.Copyright = new CopyrightInfo("Matej Urbas", 2015);
      helpText.AddPreOptionsLine("\nUsage: bud [Options] [--] command1 command2 ... commandN");
      helpText.AddPreOptionsLine("\nOptions:");
      return helpText;
    }
  }
}