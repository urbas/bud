using System;
using System.Threading.Tasks;
using Bud.Configuration;
using Moq;
using NUnit.Framework;

namespace Bud {
  public class ConfigsTest {
    private static readonly Key<int> A = nameof(A);
    private static readonly Key<int> B = nameof(B);
    private static readonly Key<int> C = nameof(C);
    private static readonly Key<Task<int>> BAsync = nameof(BAsync);
    private static readonly Key<string> AString = A.Id;

    [Test]
    public void Empty() => Assert.IsEmpty(Conf.Empty);

    [Test]
    public void Add_must_append_the_config_transformation() {
      var configTransform = new Mock<IConfigTransform>().Object;
      Assert.That(Conf.Empty.Add(configTransform), Contains.Item(configTransform));
    }

    [Test]
    public void Const_defines_a_constant_valued_configuration() {
      Assert.AreEqual(42, Conf.Empty.Const(A, 42).Get(A));
    }

    [Test]
    public void Const_redefines_configurations() {
      Assert.AreEqual(1, Conf.Empty.Const(A, 42).Const(A, 1).Get(A));
    }

    [Test]
    public void InitConst_defines_a_constant_valued_configuration() {
      Assert.AreEqual(42, Conf.Empty.InitConst(A, 42).Get(A));
    }

    [Test]
    public void InitConst_does_not_redefine_configurations() {
      Assert.AreEqual(42, Conf.Empty.InitConst(A, 42).InitConst(A, 1).Get(A));
    }

    [Test]
    public void Redefined_configuration_is_never_invoked() {
      var intConfig = new Mock<Func<IConf, int>>();
      Assert.AreEqual(1, Conf.Empty.Init(A, intConfig.Object).Const(A, 1).Get(A));
      intConfig.Verify(self => self(It.IsAny<IConf>()), Times.Never());
    }

    [Test]
    public void Modified_configurations_modify_old_values() {
      var value = Conf.Empty.InitConst(A, 42).Modify(A, (configs, oldConfig) => oldConfig + 1).Get(A);
      Assert.AreEqual(43, value);
    }

    [Test]
    public void Throw_when_modifying_a_configuration_that_does_not_yet_exist() {
      var exception = Assert.Throws<ConfigDefinitionException>(
        () => Conf.Empty.Modify(A, (configs, oldConfig) => oldConfig + 1).Bake());
      Assert.AreEqual(A.Id, exception.Key);
    }

    [Test]
    public void Throw_when_requiring_the_wrong_value_type() {
      var exception = Assert.Throws<ConfigTypeException>(
        () => Conf.Empty.Const(AString, "foo").Get(A));
      Assert.That(exception.Message, Contains.Substring(A.Id));
      Assert.That(exception.Message, Contains.Substring("System.Int32"));
      Assert.That(exception.Message, Contains.Substring("System.String"));
    }

    [Test]
    public void Defining_and_then_invoking_two_configurations_must_return_both_of_their_values() {
      var configs = Conf.Empty.Const(A, 42).Const(B, 1337);
      Assert.AreEqual(42, configs.Get(A));
      Assert.AreEqual(1337, configs.Get(B));
    }

    [Test]
    public void Invoking_a_single_configuration_must_not_invoke_the_other() {
      var intConfig = new Mock<Func<IConf, int>>();
      var configs = Conf.Empty.Set(A, intConfig.Object).Const(B, 1337);
      configs.Get(B);
      intConfig.Verify(self => self(It.IsAny<IConf>()), Times.Never);
    }

    [Test]
    public void Extended_configs_must_contain_configurations_from_the_original_as_well_as_extending_configs() {
      var originalConfigs = Conf.Empty.Const(A, 42);
      var extendingConfigs = Conf.Empty.Const(B, 58);
      var combinedConfigs = originalConfigs.Add(extendingConfigs);
      Assert.AreEqual(42, combinedConfigs.Get(A));
      Assert.AreEqual(58, combinedConfigs.Get(B));
    }

    [Test]
    public void Invoking_an_undefined_config_must_throw_an_exception() {
      var actualException = Assert.Throws<ConfigUndefinedException>(() => Conf.Empty.Get(AString));
      Assert.That(actualException.Message, Contains.Substring(A));
    }

    [Test]
    public void A_config_can_invoke_another_config() {
      var configResult = Conf.Empty.Const(A, 42)
                                .Set(B, configs => configs.Get(A) + 1)
                                .Get(B);
      Assert.AreEqual(43, configResult);
    }

    [Test]
    public void Configurations_are_invoked_once_only_and_their_result_is_cached() {
      var intConfig = new Mock<Func<IConf, int>>();
      Conf.Empty.Set(A, intConfig.Object)
             .Set(B, configs => A[configs] + A[configs])
             .Get(B);
      intConfig.Verify(self => self(It.IsAny<IConf>()));
    }

    [Test]
    public async void Multithreaded_access_to_configs_should_not_result_in_duplicate_invocations() {
      var intConfig = new Mock<Func<IConf, int>>();
      var configs = Conf.Empty.Set(A, intConfig.Object)
                           .Set(BAsync, AddFooTwiceConcurrently);
      int repeatCount = 10;
      for (int i = 0; i < repeatCount; i++) {
        await configs.Get(BAsync);
      }
      intConfig.Verify(self => self(It.IsAny<IConf>()), Times.Exactly(repeatCount));
    }

    [Test]
    public void Nesting_prefixes_config_names() {
      var nestedConfigs = "bar" / Conf.Empty.Const(A, 42);
      Assert.AreEqual(42, nestedConfigs.Get("bar" / A));
    }

    [Test]
    public void Nesting_allows_access_to_sibling_configs() {
      var nestedConfigs = "bar" / Conf.Empty.Set(B, configs => A[configs] + 1)
                                         .Const(A, 42);
      Assert.AreEqual(43, nestedConfigs.Get("bar" / B));
    }

    [Test]
    public void Nested_configs_can_be_modified() {
      var nestedConfigs = "bar" / Conf.Empty.Const(A, 42);
      var modifiedNestedConfigs = nestedConfigs.Modify("bar" / A, (configs, i) => 58);
      Assert.AreEqual(58, modifiedNestedConfigs.Get("bar" / A));
    }

    [Test]
    public void Nesting_with_multiple_modifications() {
      var nestedConfigs = "bar" / Conf.Empty
                                         .InitConst(C, 10)
                                         .Init(B, configs => 1)
                                         .Modify(B, (configs, i) => i + C[configs])
                                         .Modify(B, (configs, i) => i + C[configs]);
      Assert.AreEqual(21, nestedConfigs.Get("bar" / B));
    }

    private static async Task<int> AddFooTwiceConcurrently(IConf tsks) {
      var first = Task.Run(() => tsks.Get(A));
      var second = Task.Run(() => tsks.Get(A));
      return await first + await second;
    }
  }
}