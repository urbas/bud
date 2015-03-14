using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace Bud {
  public class CliArguments {
    [Option('b', "build-level", DefaultValue = 0, HelpText = "The build configuration level at which to evaluate tasks. Level 0 corresponds to the build configuration defined in '.bud/Build.cs', level 1 to '.bud/.bud/Build.cs' and so on.")]
    public int BuildLevel { get; set; }

    [ValueList(typeof(List<string>))]
    public IList<string> Commands { get; set; }

    public string GetUsage() {
      var helpText = HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
      helpText.Heading = new HeadingInfo("Bud: the build tool without the 'ill'.");
      helpText.Copyright = new CopyrightInfo("Matej Urbas", 2015);
      helpText.AddPreOptionsLine("\nUsage: bud [Options] command1 command2 ... commandN");
      helpText.AddPreOptionsLine("\nOptions:");
      return helpText;
    }
  }
}