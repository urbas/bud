using System;
using System.Threading.Tasks;
using Bud.Configuration;
using Moq;
using NUnit.Framework;

namespace Bud {
  public class ConfigsTest {
    private readonly Key<int> fooInt = "foo";
    private readonly Key<int> barInt = nameof(barInt);
    private readonly Key<Task<int>> barAsyncInt = nameof(barAsyncInt);
    private readonly Key<string> fooString = "foo";

    [Test]
    public void Empty() => Assert.IsEmpty(Configs.Empty);

    [Test]
    public void Add_must_append_the_config_transformation() {
      var configTransform = new Mock<IConfigTransform>().Object;
      Assert.That(Configs.Empty.Add(configTransform), Contains.Item(configTransform));
    }

    [Test]
    public void Const_defines_a_constant_valued_configuration() {
      Assert.AreEqual(42, Configs.Empty.Const(fooInt, 42).Get(fooInt));
    }

    [Test]
    public void Const_redefines_configurations() {
      Assert.AreEqual(1, Configs.Empty.Const(fooInt, 42).Const(fooInt, 1).Get(fooInt));
    }

    [Test]
    public void InitConst_defines_a_constant_valued_configuration() {
      Assert.AreEqual(42, Configs.Empty.InitConst(fooInt, 42).Get(fooInt));
    }

    [Test]
    public void InitConst_does_not_redefine_configurations() {
      Assert.AreEqual(42, Configs.Empty.InitConst(fooInt, 42).InitConst(fooInt, 1).Get(fooInt));
    }

    [Test]
    public void Redefined_configuration_is_never_invoked() {
      var intConfig = new Mock<Func<IConfigs, int>>();
      Assert.AreEqual(1, Configs.Empty.Init(fooInt, intConfig.Object).Const(fooInt, 1).Get(fooInt));
      intConfig.Verify(self => self(It.IsAny<IConfigs>()), Times.Never());
    }

    [Test]
    public void Modified_configurations_modify_old_values() {
      var value = Configs.Empty.InitConst(fooInt, 42).Modify(fooInt, (configs, oldConfig) => oldConfig + 1).Get(fooInt);
      Assert.AreEqual(43, value);
    }

    [Test]
    public void Throw_when_modifying_a_configuration_that_does_not_yet_exist() {
      var exception = Assert.Throws<ConfigDefinitionException>(
        () => Configs.Empty.Modify(fooInt, (configs, oldConfig) => oldConfig + 1).Bake());
      Assert.AreEqual(fooInt.Id, exception.Key);
      Assert.AreEqual(fooInt.Type, exception.ValueType);
    }

    [Test]
    public void Throw_when_requiring_the_wrong_value_type() {
      var exception = Assert.Throws<ConfigTypeException>(
        () => Configs.Empty.Const(fooString, "foo").Get(fooInt));
      Assert.That(exception.Message, Contains.Substring(fooInt.Id));
      Assert.That(exception.Message, Contains.Substring("System.Int32"));
      Assert.That(exception.Message, Contains.Substring("System.String"));
    }

    [Test]
    public void Defining_and_then_invoking_two_configurations_must_return_both_of_their_values() {
      var configs = Configs.Empty.Const(fooInt, 42).Const(barInt, 1337);
      Assert.AreEqual(42, configs.Get(fooInt));
      Assert.AreEqual(1337, configs.Get(barInt));
    }

    [Test]
    public void Invoking_a_single_configuration_must_not_invoke_the_other() {
      var intConfig = new Mock<Func<IConfigs, int>>();
      var configs = Configs.Empty.Set(fooInt, intConfig.Object).Const(barInt, 1337);
      configs.Get(barInt);
      intConfig.Verify(self => self(It.IsAny<IConfigs>()), Times.Never);
    }

    [Test]
    public void Extended_configs_must_contain_configurations_from_the_original_as_well_as_extending_configs() {
      var originalConfigs = Configs.Empty.Const(fooInt, 42);
      var extendingConfigs = Configs.Empty.Const(barInt, 58);
      var combinedConfigs = originalConfigs.Add(extendingConfigs);
      Assert.AreEqual(42, combinedConfigs.Get(fooInt));
      Assert.AreEqual(58, combinedConfigs.Get(barInt));
    }

    [Test]
    public void Invoking_an_undefined_config_must_throw_an_exception() {
      var actualException = Assert.Throws<ConfigUndefinedException>(() => Configs.Empty.Get(fooString));
      Assert.That(actualException.Message, Contains.Substring(fooInt));
    }

    [Test]
    public void A_config_can_invoke_another_config() {
      var configResult = Configs.Empty.Const(fooInt, 42)
                                .Set(barInt, configs => configs.Get(fooInt) + 1)
                                .Get(barInt);
      Assert.AreEqual(43, configResult);
    }

    [Test]
    public void Configurations_are_invoked_once_only_and_their_result_is_cached() {
      var intConfig = new Mock<Func<IConfigs, int>>();
      Configs.Empty.Set(fooInt, intConfig.Object)
             .Set(barInt, configs => fooInt[configs] + fooInt[configs])
             .Get(barInt);
      intConfig.Verify(self => self(It.IsAny<IConfigs>()));
    }

    [Test]
    public async void Multithreaded_access_to_configs_should_not_result_in_duplicate_invocations() {
      var intConfig = new Mock<Func<IConfigs, int>>();
      var configs = Configs.Empty.Set(fooInt, intConfig.Object)
                           .Set(barAsyncInt, AddFooTwiceConcurrently);
      int repeatCount = 10;
      for (int i = 0; i < repeatCount; i++) {
        await configs.Get(barAsyncInt);
      }
      intConfig.Verify(self => self(It.IsAny<IConfigs>()), Times.Exactly(repeatCount));
    }

    [Test]
    public void Nesting_prefixes_config_names() {
      var nestedConfigs = Configs.Empty.Const(fooInt, 42).Nest("bar");
      Assert.AreEqual(42, nestedConfigs.Get("bar" / fooInt));
    }

    [Test]
    public void Nesting_allows_access_to_sibling_configs() {
      var nestedConfigs = Configs.Empty.Set(barInt, configs => fooInt[configs] + 1)
                                 .Const(fooInt, 42)
                                 .Nest("bar");
      Assert.AreEqual(43, nestedConfigs.Get("bar" / barInt));
    }

    [Test]
    public void Nested_configs_can_be_modified() {
      var nestedConfigs = Configs.Empty
                                 .Const(fooInt, 42)
                                 .Nest("bar")
                                 .Modify("bar" / fooInt, (configs, i) => 58);
      Assert.AreEqual(58, nestedConfigs.Get("bar" / fooInt));
    }

    private async Task<int> AddFooTwiceConcurrently(IConfigs tsks) {
      var first = Task.Run(() => tsks.Get(fooInt));
      var second = Task.Run(() => tsks.Get(fooInt));
      return await first + await second;
    }
  }
}