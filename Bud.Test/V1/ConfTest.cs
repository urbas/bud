using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Bud.Configuration;
using Moq;
using NUnit.Framework;
using static Bud.V1.Conf;

namespace Bud.V1 {
  public class ConfTest {
    private static readonly Key<int> A = nameof(A);
    private static readonly Key<int> B = nameof(B);
    private static readonly Key<Task<int>> BAsync = nameof(BAsync);
    private static readonly Key<string> AString = A.Id;
    private static readonly Key<IEnumerable<int>> FooEnumerable = "Foo";
    private static readonly Key<IList<int>> FooList = "Foo";

    [Test]
    public void Const_defines_a_constant_valued_configuration()
      => Assert.AreEqual(42, A.SetValue(42).Get(A));

    [Test]
    public void Const_redefines_configurations()
      => Assert.AreEqual(1, A.SetValue(42).SetValue(A, 1).Get(A));

    [Test]
    public void InitConst_defines_a_constant_valued_configuration()
      => Assert.AreEqual(42, A.InitValue(42).Get(A));

    [Test]
    public void InitConst_does_not_redefine_configurations()
      => Assert.AreEqual(42, A.InitValue(42).InitValue(A, 1).Get(A));

    [Test]
    public void Redefined_configuration_is_never_invoked() {
      var intConfig = new Mock<Func<IConf, int>>();
      Assert.AreEqual(1, A.Init(intConfig.Object).SetValue(A, 1).Get(A));
      intConfig.Verify(self => self(It.IsAny<IConf>()), Times.Never());
    }

    [Test]
    public void Modified_configurations_modify_old_values()
      => Assert.AreEqual(43, A.InitValue(42)
                              .Modify(A, (configs, oldConfig) => oldConfig + 1)
                              .Get(A));

    [Test]
    public void Throw_when_modifying_a_configuration_that_does_not_yet_exist() {
      var exception = Assert.Throws<ConfDefinitionException>(
        () => A.Modify((configs, oldConfig) => oldConfig + 1).Get(A));
      Assert.AreEqual(A.Id, exception.Key);
      Assert.That(exception.Message, Contains.Substring(A.Id));
    }

    [Test]
    public void Throw_when_requiring_the_wrong_value_type()
      => Assert.Throws<InvalidCastException>(() => AString.SetValue("foo").Get(A));

    [Test]
    public void Get_a_subtype() {
      var expectedList = ImmutableArray.Create(42);
      Assert.AreEqual(expectedList, FooEnumerable.InitValue(expectedList)
                                                 .Get(FooList));
    }

    [Test]
    public void Defining_and_then_invoking_two_configurations_must_return_both_of_their_values() {
      var conf = A.SetValue(42)
                  .SetValue(B, 1337);
      Assert.AreEqual(42, conf.Get(A));
      Assert.AreEqual(1337, conf.Get(B));
    }

    [Test]
    public void Invoking_a_single_configuration_must_not_invoke_the_other() {
      var intConfig = new Mock<Func<IConf, int>>();
      A.Set(intConfig.Object)
       .SetValue(B, 1337)
       .Get(B);
      intConfig.Verify(self => self(It.IsAny<IConf>()), Times.Never);
    }

    [Test]
    public void Extended_configs_must_contain_configurations_from_the_original_as_well_as_extending_configs() {
      var confA = A.SetValue(42);
      var confB = B.SetValue(58);
      var combinedConfigs = confA.Add(confB);
      Assert.AreEqual(42, combinedConfigs.Get(A));
      Assert.AreEqual(58, combinedConfigs.Get(B));
    }

    [Test]
    public void Invoking_an_undefined_config_must_throw_an_exception() {
      var actualException = Assert.Throws<ConfUndefinedException>(() => Empty.Get(AString));
      Assert.That(actualException.Message, Contains.Substring(A));
    }

    [Test]
    public void A_config_can_invoke_another_config()
      => Assert.AreEqual(43, A.SetValue(42)
                              .Set(B, configs => configs.Get(A) + 1)
                              .Get(B));

    [Test]
    public void Configurations_are_invoked_once_only_and_their_result_is_cached() {
      var intConfig = new Mock<Func<IConf, int>>();
      A.Set(intConfig.Object)
       .Set(B, configs => A[configs] + A[configs])
       .Get(B);
      intConfig.Verify(self => self(It.IsAny<IConf>()));
    }

    [Test]
    public async void Multithreaded_access_to_configs_should_not_result_in_duplicate_invocations() {
      var intConfig = new Mock<Func<IConf, int>>();
      var conf = A.Set(intConfig.Object)
                  .Set(BAsync, AddFooTwiceConcurrently);
      const int repeatCount = 10;
      for (int i = 0; i < repeatCount; i++) {
        await conf.Get(BAsync);
      }
      intConfig.Verify(self => self(It.IsAny<IConf>()), Times.Exactly(repeatCount));
    }

    [Test]
    public void Empty_configuration_has_no_scope()
      => Assert.IsEmpty(Empty.Scope);

    [Test]
    public void Descending_into_subscope()
      => Assert.AreEqual(new[] {"foo"}, Empty.In("foo").Scope);

    [Test]
    public void Multiple_subscope_descend()
      => Assert.AreEqual(new[] {"foo", "bar"}, Empty.In("foo").In("bar").Scope);

    [Test]
    public void Scope_backtracking()
      => Assert.IsEmpty(Group("foo").Out().Scope);

    [Test]
    public void Throws_when_backtracking_from_empty_scope()
      => Assert.Throws<InvalidOperationException>(() => Empty.Out());

    [Test]
    public void Getting_values_of_nested_configurations() {
      var subConf = Group("foo").SetValue(A, 42);
      Assert.AreEqual(42, subConf.Out().Get("foo" / A));
    }

    [Test]
    public void Getting_siblings_in_nested_configurations() {
      var subConf = Group("foo").SetValue(A, 42)
                                .Set(B, conf => 1 + A[conf]);
      Assert.AreEqual(43, subConf.Get(B));
    }

    [Test]
    public void Modifying_values_of_double_nested_configurations() {
      var subConf = Group("foo").In("bar")
                                .SetValue(A, 42)
                                .Modify(A, (conf, oldValue) => 1 + oldValue);
      Assert.AreEqual(43, subConf.Get(A));
    }

    [Test]
    public void Nested_configurations_can_access_configurations_by_absolute_path() {
      var confA = A.SetValue(42);
      var confB = Group("foo").Set(B, conf => 1 + conf.Get(Keys.Root / A));
      Assert.AreEqual(43, Group(confA, confB).Get("foo" / B));
    }

    [Test]
    public void Nested_configurations_can_access_configurations_by_relative_path() {
      var confA = A.SetValue(42);
      var confB = Group("foo").Set(B, conf => 1 + conf.Get(".." / A));
      Assert.AreEqual(43, Group(confA, confB).Get("foo" / B));
    }

    [Test]
    public void Doubly_nested_configurations_can_access_configurations_by_relative_path() {
      var confA = A.SetValue(42);
      var confB = Group("foo").In("bar").Set(B, conf => 1 + conf.Get("../.." / A));
      Assert.AreEqual(43, Group(confA, confB).Get("foo/bar" / B));
    }

    [Test]
    public void Triply_nested_configurations_can_access_configurations_by_relative_path() {
      var confA = A.SetValue(42);
      var confB = Group("foo").In("bar").Set(B, conf => 1 + conf.Get("../../../moo" / A));
      Assert.AreEqual(43, Group("moo").Add(confA, confB).Get("foo/bar" / B));
    }

    [Test]
    public void Nested_configurations_can_access_configurations_in_other_branches_via_relative_paths() {
      var confA = Group("foo").In("boo").SetValue(A, 42);
      var confB = Group("foo").In("bar").Set(B, conf => 1 + conf.Get("../boo" / A));
      Assert.AreEqual(43, Empty.Add(confA, confB).Get("foo/bar" / B));
    }

    [Test]
    public void Root_config_can_be_accessed_from_current_scope()
      => Assert.AreEqual(42, (Keys.Root / A).SetValue(42).Get(A));

    [Test]
    public void Relative_reference_in_a_nested_modified_conf() {
      var confA = Group("a").InitValue(A, 42).InitValue(B, 1);
      var confB = Empty.Modify(B, (conf, oldValue) => oldValue + A[conf]);
      Assert.AreEqual(43, confA.Add(confB).Get(B));
    }

    [Test]
    public void Relative_reference_in_a_nested_and_doubly_modified_conf() {
      var confA = Group("a").InitValue(A, 42).InitValue(B, 1);
      var confB = Empty.Modify(B, (conf, oldValue) => oldValue + A[conf]);
      Assert.AreEqual(85, confA.Add(confB, confB).Get(B));
    }

    [Test]
    public void Relative_reference_in_a_nested_and_modified_conf_defined_in_another_scope() {
      var confA = Group("a").InitValue(A, 42);
      var confB0 = Group("e").In("f").In("g").Set("../../.." / B, conf => 1 + ("../../.." / A)[conf]);
      var confB1 = Group("b").Modify("../a" / B, (conf, oldValue) => oldValue + ("../a" / A)[conf]);
      var confB2 = Group("c").In("d").Modify("../../a" / B, (conf, oldValue) => oldValue + ("../../a" / A)[conf]);
      Assert.AreEqual(253, Empty.Add(confA.Add(confB0), confB0, confB1, confB2, confB1, confB2, confB2).Get("a" / B));
    }

    [Test]
    public void Calculate_value_only_once_when_invoking_relative_references_multiple_times() {
      var valueFactoryA = new Mock<Func<IConf, int>>(MockBehavior.Strict);
      valueFactoryA.Setup(self => self(It.IsAny<IConf>())).Returns(42);
      var conf = Group(Group("a").Set(A, valueFactoryA.Object),
                       Group("b").Set(B, c => c.Get("../a" / A)))
        .ToCompiled();
      conf.Get("b" / B);
      conf.Get("b" / B);
      valueFactoryA.VerifyAll();
    }

    [Test]
    public void TryGet_returns_an_empty_optional_for_undefined_keys()
      => Assert.IsFalse(Empty.TryGet(A).HasValue);

    [Test]
    public void TryGet_returns_an_optional_with_a_value_when_the_key_is_defined()
      => Assert.IsTrue(Empty.SetValue(A, 42).TryGet(A).HasValue);

    [Test]
    public void TryGet_returns_an_optional_containing_the_value_of_the_key()
      => Assert.AreEqual(42, Empty.SetValue(A, 42).TryGet(A).Value);

    private static async Task<int> AddFooTwiceConcurrently(IConf conf) {
      var first = Task.Run(() => A[conf]);
      var second = Task.Run(() => A[conf]);
      return await first + await second;
    }
  }
}