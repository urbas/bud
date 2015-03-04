using Bud.Build;

namespace Bud.Resources {
  public static class Res {
    public static Setup Main(params Setup[] setups) => new ResourcesBuildTarget(BuildKeys.Main, setups).Setup;
    public static Setup Test(params Setup[] setups) => new ResourcesBuildTarget(BuildKeys.Test, setups).Setup;
  }
}