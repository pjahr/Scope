using System;
using System.IO;

namespace Scope.Deserialization
{
  class CryXmlHeader
  {
    public CryXmlHeader(BinaryReader r)
    {
      string header = r.ReadFString(8);

      if (header != "CryXmlB")
      {
        throw new FormatException("Unknown File Format");
      }

      XmlSize = r.ReadInt();

      NodeTableOffset = r.ReadInt();
      NodeTableCount = r.ReadInt();

      AttributeTableOffset = r.ReadInt();
      AttributeTableCount = r.ReadInt();

      ChildTableOffset = r.ReadInt();
      ChildTableCount = r.ReadInt();

      StringTableOffset = r.ReadInt();
      StringTableSize = r.ReadInt();
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
