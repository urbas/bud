using System;
using System.IO;
using System.Reflection;
using System.Text;
using Bud.Build;
using Bud.Logging;

namespace Bud.Commander {
  public class AssemblyBuildCommander : MarshalByRefObject, IBuildCommander {
    private Settings Settings;
    private IConfig Config;

    public void LoadBuildConfiguration(string buildConfigurationAssemblyFile, string baseDirectory, TextWriter standardOutputTextWriter, TextWriter standardErrorTextWriter) {
      Console.SetOut(new NonSerializingOutputWriter(standardOutputTextWriter));
      Console.SetError(new NonSerializingOutputWriter(standardErrorTextWriter));
      var assembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(buildConfigurationAssemblyFile));
      var build = (IBuild) assembly.CreateInstance("Build");
      Settings = build.Setup(GlobalBuild.New(baseDirectory), baseDirectory);
      Config = new Config(Settings.ConfigDefinitions, Logger.CreateFromWriters(standardOutputTextWriter, standardErrorTextWriter));
    }

    public object Evaluate(string command) {
      var context = Context.FromConfig(Config, Settings.TaskDefinitions);
      return CommandEvaluator.EvaluateSynchronously(context, command);
    }

    public void Dispose() {}
  }

  public class NonSerializingOutputWriter : TextWriter {
    private readonly TextWriter StandardOutputTextWriter;

    public NonSerializingOutputWriter(TextWriter standardOutputTextWriter) {
      StandardOutputTextWriter = standardOutputTextWriter;
    }

    public override Encoding Encoding {
      get { return StandardOutputTextWriter.Encoding; }
    }

    public override void Write(object value) {
      base.Write(value.ToString());
    }
  }
}