using System;
using System.Threading.Tasks;
using Bud.Configuration;
using Moq;
using NUnit.Framework;

namespace Bud {
  public class ConfigExtensionsTest {
    private readonly Key<int> fooInt = "foo";
    private readonly Key<int> barInt = nameof(barInt);
    private readonly Key<Task<int>> barAsyncInt = nameof(barAsyncInt);
    private readonly Key<string> fooString = "foo";

    [Test]
    public void Empty() => Assert.IsEmpty(Configs.NewConfigs);

    [Test]
    public void Add_must_append_the_config_transformation() {
      var configTransform = new Mock<IConfigTransform>().Object;
      Assert.That(Configs.NewConfigs.Add(configTransform), Contains.Item(configTransform));
    }

    [Test]
    public void Const_defines_a_constant_valued_configuration() {
      Assert.AreEqual(42, Configs.NewConfigs.Const(fooInt, 42).Get(fooInt));
    }

    [Test]
    public void Const_redefines_configurations() {
      Assert.AreEqual(1, Configs.NewConfigs.Const(fooInt, 42).Const(fooInt, 1).Get(fooInt));
    }

    [Test]
    public void InitConst_defines_a_constant_valued_configuration() {
      Assert.AreEqual(42, Configs.NewConfigs.InitConst(fooInt, 42).Get(fooInt));
    }

    [Test]
    public void InitConst_does_not_redefine_configurations() {
      Assert.AreEqual(42, Configs.NewConfigs.InitConst(fooInt, 42).InitConst(fooInt, 1).Get(fooInt));
    }

    [Test]
    public void Redefined_configuration_is_never_invoked() {
      var intConfig = new Mock<Func<IConfigs, int>>();
      Assert.AreEqual(1, Configs.NewConfigs.Init(fooInt, intConfig.Object).Const(fooInt, 1).Get(fooInt));
      intConfig.Verify(self => self(It.IsAny<IConfigs>()), Times.Never());
    }

    [Test]
    public void Modified_configurations_modify_old_values() {
      var value = Configs.NewConfigs.InitConst(fooInt, 42).Modify(fooInt, (configs, oldConfig) => oldConfig + 1).Get(fooInt);
      Assert.AreEqual(43, value);
    }

    [Test]
    public void Throw_when_modifying_a_configuration_that_does_not_yet_exist() {
      var exception = Assert.Throws<ConfigDefinitionException>(
        () => Configs.NewConfigs.Modify(fooInt, (configs, oldConfig) => oldConfig + 1).Compile());
      Assert.AreEqual(fooInt.Id, exception.Key);
      Assert.AreEqual(fooInt.Type, exception.ValueType);
    }

    [Test]
    public void Throw_when_requiring_the_wrong_value_type() {
      var exception = Assert.Throws<ConfigTypeException>(
        () => Configs.NewConfigs.Const(fooString, "foo").Get(fooInt));
      Assert.That(exception.Message, Contains.Substring(fooInt.Id));
      Assert.That(exception.Message, Contains.Substring("System.Int32"));
      Assert.That(exception.Message, Contains.Substring("System.String"));
    }

    [Test]
    public void Defining_and_then_invoking_two_configurations_must_return_both_of_their_values() {
      var configs = Configs.NewConfigs.Const(fooInt, 42).Const(barInt, 1337);
      Assert.AreEqual(42, configs.Get(fooInt));
      Assert.AreEqual(1337, configs.Get(barInt));
    }

    [Test]
    public void Invoking_a_single_configuration_must_not_invoke_the_other() {
      var intConfig = new Mock<Func<IConfigs, int>>();
      var configs = Configs.NewConfigs.Set(fooInt, intConfig.Object).Const(barInt, 1337);
      configs.Get(barInt);
      intConfig.Verify(self => self(It.IsAny<IConfigs>()), Times.Never);
    }

    [Test]
    public void Extended_configs_must_contain_configurations_from_the_original_as_well_as_extending_configs() {
      var originalConfigs = Configs.NewConfigs.Const(fooInt, 42);
      var extendingConfigs = Configs.NewConfigs.Const(barInt, 58);
      var combinedConfigs = originalConfigs.ExtendWith(extendingConfigs);
      Assert.AreEqual(42, combinedConfigs.Get(fooInt));
      Assert.AreEqual(58, combinedConfigs.Get(barInt));
    }

    [Test]
    public void Invoking_an_undefined_config_must_throw_an_exception() {
      var actualException = Assert.Throws<ConfigUndefinedException>(() => Configs.NewConfigs.Get(fooString));
      Assert.That(actualException.Message, Contains.Substring(fooInt));
    }

    [Test]
    public void A_config_can_invoke_another_config() {
      var configResult = Configs.NewConfigs.Const(fooInt, 42)
                               .Set(barInt, configs => configs.Get(fooInt) + 1)
                               .Get(barInt);
      Assert.AreEqual(43, configResult);
    }

    [Test]
    public void Configurations_are_invoked_once_only_and_their_result_is_cached() {
      var intTask = new Mock<Func<IConfigs, int>>();
      Configs.NewConfigs.Set(fooInt, intTask.Object)
              .Set(barInt, configs => fooInt[configs] + fooInt[configs])
              .Get(barInt);
      intTask.Verify(self => self(It.IsAny<IConfigs>()));
    }

    [Test]
    public async void Multithreaded_access_to_configs_should_not_result_in_duplicate_invocations() {
      var intTask = new Mock<Func<IConfigs, int>>();
      var configs = Configs.NewConfigs.Set(fooInt, intTask.Object)
                          .Set(barAsyncInt, AddFooTwiceConcurrently);
      int repeatCount = 10;
      for (int i = 0; i < repeatCount; i++) {
        await configs.Get(barAsyncInt);
      }
      intTask.Verify(self => self(It.IsAny<IConfigs>()), Times.Exactly(repeatCount));
    }

    [Test]
    public void Nesting_prefixes_config_names() {
      var nestedConfigs = Configs.NewConfigs.Const(fooInt, 42).Nest("bar");
      Assert.AreEqual(42, nestedConfigs.Get("bar" / fooInt));
    }

    [Test]
    public void Nesting_allows_access_to_sibling_configs() {
      var nestedConfigs = Configs.NewConfigs.Set(barInt, configs => fooInt[configs] + 1)
                                .Const(fooInt, 42)
                                .Nest("bar");
      Assert.AreEqual(43, nestedConfigs.Get("bar" / barInt));
    }

    private async Task<int> AddFooTwiceConcurrently(IConfigs tsks) {
      var first = Task.Run(() => tsks.Get(fooInt));
      var second = Task.Run(() => tsks.Get(fooInt));
      return await first + await second;
    }
  }
}