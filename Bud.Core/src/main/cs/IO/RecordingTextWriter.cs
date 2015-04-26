using System.IO;
using System.Text;

namespace Bud.IO {
  public class RecordingTextWriter : TextWriter {
    private readonly TextWriter PipedTextWriter;
    private readonly StringWriter RecordingStringWriter = new StringWriter();

    public RecordingTextWriter(TextWriter pipedTextWriter) {
      PipedTextWriter = pipedTextWriter;
    }

    public override Encoding Encoding => PipedTextWriter.Encoding;

    public override void Write(char value) {
      PipedTextWriter.Write(value);
      RecordingStringWriter.Write(value);
    }

    public override string ToString() => RecordingStringWriter.ToString();
  }
}