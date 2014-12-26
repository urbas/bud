using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Bud.Commander {
  public class Evaluation {
    public Evaluation(Task<string> result) {
      Result = result;
    }

    public Task<string> Result { get; private set; }
  }
}
