using System.IO;

namespace Scope.Deserialization
{
  public class CryXmlValue
  {
    public CryXmlValue(BinaryReader r, int stringTableOffset)
    {
      var position = r.BaseStream.Position;
      Offset = (int)position - stringTableOffset;
      Value = BinaryReaderExtensions.ReadCString(r);
    }

    public int Offset { get; set; }
    public string Value { get; set; }

    public override string ToString() => $"N:{Offset} V:{Value}";
  }
}
