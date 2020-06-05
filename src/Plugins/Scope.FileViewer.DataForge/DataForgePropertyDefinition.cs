using System;
using System.IO;

namespace Scope.FileViewer.DataForge
{
  internal class DataForgePropertyDefinition
  {
    public DataForgePropertyDefinition(BinaryReader r)
    {
      this.NameOffset = r.ReadUInt32();
      this.StructIndex = r.ReadUInt16();
      this.DataType = (EDataType)r.ReadUInt16();
      this.ConversionType = (EConversionType)r.ReadUInt16();
      this.Padding = r.ReadUInt16();
    }

    public uint NameOffset { get; set; }
    public ushort StructIndex { get; set; }
    public EDataType DataType { get; set; }
    public EConversionType ConversionType { get; set; }
    public ushort Padding { get; set; }

    //public String Name { get { return this.DocumentRoot.ValueMap[this.NameOffset]; } }
  }
}