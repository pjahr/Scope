using System;
using System.IO;

namespace Scope.FileViewer.DataForge.Models
{
  internal class PropertyDefinition
  {
    private readonly Func<uint, string> _valueOf;

    public PropertyDefinition(BinaryReader r, Func<uint, string> valueOf)
    {
      _valueOf = valueOf;

      NameOffset = r.ReadUInt32();
      StructIndex = r.ReadUInt16();
      DataType = (DataType)r.ReadUInt16();
      ConversionType = (ConversionType)r.ReadUInt16();
      Padding = r.ReadUInt16();
    }

    public uint NameOffset { get; set; }
    public ushort StructIndex { get; set; }
    public DataType DataType { get; set; }
    public ConversionType ConversionType { get; set; }
    public ushort Padding { get; set; }

    public string Name => _valueOf(NameOffset);
  }
}