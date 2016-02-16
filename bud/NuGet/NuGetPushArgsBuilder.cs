using System.Text;
using Bud.Util;

namespace Bud.NuGet {
  public class NuGetPushArgsBuilder {
    public virtual string CreateArgs(string package,
                                     Option<string> targetUrl,
                                     Option<string> apiKey) {
      var sb = new StringBuilder("push ");
      sb.Append(package);
      if (targetUrl.HasValue) {
        sb.Append(" -Source ").Append(targetUrl.Value);
      }
      if (apiKey.HasValue) {
        sb.Append(" -ApiKey ").Append(apiKey.Value);
      }
      return sb.ToString();
    }
  }
}