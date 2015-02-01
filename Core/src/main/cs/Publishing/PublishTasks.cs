using System.Threading.Tasks;
using NuGet;

namespace Bud.Publishing {
  public static class PublishTasks {
    public static async Task<string> Package(IContext context, Key buildTarget) {
      return await context.Evaluate(buildTarget / PublishKeys.Package);
    }
  }
}