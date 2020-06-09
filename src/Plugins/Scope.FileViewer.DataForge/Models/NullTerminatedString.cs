using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  internal class NullTerminatedString
  {
    public NullTerminatedString(BinaryReader r)
    {
      Value = r.ReadNullTerminatedString();
    }

    string Value { get; }
  }
}