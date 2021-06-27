using System;
using System.IO;

namespace Scope.Deserialization
{
  class CryXmlHeader
  {
    private readonly ByteOrderEnum _byteOrder;

    public CryXmlHeader(BinaryReader r, ByteOrderEnum byteOrder)
    {
      _byteOrder = byteOrder;

      string header = r.ReadFString(8);

      if (header != "CryXmlB")
      {
        throw new FormatException("Unknown File Format");
      }

      XmlSize = r.ReadInt(_byteOrder);

      NodeTableOffset = r.ReadInt(_byteOrder);
      NodeTableCount = r.ReadInt(_byteOrder);

      AttributeTableOffset = r.ReadInt(_byteOrder);
      AttributeTableCount = r.ReadInt(_byteOrder);

      ChildTableOffset = r.ReadInt(_byteOrder);
      ChildTableCount = r.ReadInt(_byteOrder);

      StringTableOffset = r.ReadInt(_byteOrder);
      StringTableSize = r.ReadInt(_byteOrder);
    }

    public int XmlSize { get; }
    public int NodeTableOffset { get; }
    public int NodeTableCount { get; }
    public int AttributeTableOffset { get; }
    public int AttributeTableCount { get; }
    public int ChildTableOffset { get; }
    public int ChildTableCount { get; }
    public int StringTableOffset { get; }
    public int StringTableSize { get; }
  }
}
