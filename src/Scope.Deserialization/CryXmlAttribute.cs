using System.IO;

namespace Scope.Deserialization
{
  public class CryXmlAttribute
  {
    public CryXmlAttribute(BinaryReader r)
    {
      NameOffset = r.ReadInt();
      ValueOffset = r.ReadInt();
    }

    public int NameOffset { get; }
    public int ValueOffset { get; }

    public override string ToString() => $"N:{NameOffset} V:{ValueOffset}";
  }
}