using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bud.V1;
using NUnit.Framework;
using static Bud.Cli.BuildTool;
using static Bud.Util.Option;
using static NUnit.Framework.Assert;

namespace Bud.Cli {
  public class BuildToolTest {
    [Test]
    public void ExecuteCommand_returns_None_for_undefined_keys()
      => AreEqual(None<object>(),
                  ExecuteCommand(Conf.Empty.ToCompiled(), "A"));

    [Test]
    public void ExecuteCommand_returns_the_raw_value()
      => AreEqual(42,
                  ExecuteCommand(Raw42Conf(), "A").Value);

    [Test]
    public void ExecuteCommand_returns_the_observed_value()
      => AreEqual(new[] {42},
                  ExecuteCommand(Observed42Conf(), "A").Value);

    [Test]
    public void ExecuteCommand_returns_the_result_of_the_task()
      => AreEqual(42,
                  ExecuteCommand(Task42Conf(), "A").Value);

    [Test]
    public void ExecuteCommand_returns_unit_for_void_tasks()
      => AreEqual(Some<object>(Unit.Default),
                  ExecuteCommand(TaskVoidConf(), "A"));

    private static IConf Raw42Conf()
      => Conf.Empty.InitValue("A", 42).ToCompiled();

    private static IConf Observed42Conf()
      => Conf.Empty.InitValue("A", Observable.Return(42)).ToCompiled();

    private static IConf Task42Conf()
      => Conf.Empty.InitValue("A", Task.FromResult(42)).ToCompiled();

    private static IConf TaskVoidConf()
      => Conf.Empty.InitValue("A", Task.Run(() => {})).ToCompiled();
  }
}