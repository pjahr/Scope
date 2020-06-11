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
    public object Value { get; private set; }

    public void Read(BinaryReader r, DataForgeFile df)
    {
      switch (DataType)
      {
        case DataType.Reference:
          r.ReadUInt32(); // ?
          Value = r.ReadGuid();
          break;

        case DataType.Locale:
          var offset = r.ReadUInt32();
          Value = df.ValueMap[offset];
          break;

        case DataType.StrongPointer:
          r.ReadUInt32(); // ? {1:X8}
          Value = r.ReadUInt32();
          break;

        case DataType.WeakPointer:
          var structIndex = r.ReadUInt32();
          var itemIndex = r.ReadUInt32();
          df.WeakMappings2.Add(new ClassMapping { StructIndex = (UInt16)structIndex, RecordIndex = (Int32)itemIndex });
          break;

        case DataType.String:
          var v = r.ReadUInt32();
          Value = df.ValueMap[v];
          break;

        case DataType.Boolean:
          Value = r.ReadByte();
          break;

        case DataType.Single:
          Value = r.ReadSingle();
          break;

        case DataType.Double:
          Value = r.ReadDouble();
          break;

        case DataType.Guid:
          Value = r.ReadGuid();
          break;

        case DataType.SByte:
          Value = r.ReadSByte();
          break;

        case DataType.Int16:
          Value = r.ReadInt16();
          break;

        case DataType.Int32:
          Value = r.ReadInt32();
          break;

        case DataType.Int64:
          Value = r.ReadInt64();
          break;

        case DataType.Byte:
          Value = r.ReadByte();
          break;

        case DataType.UInt16:
          Value = r.ReadUInt16();
          break;

        case DataType.UInt32:
          Value = r.ReadUInt32();
          break;

        case DataType.UInt64:
          Value = r.ReadUInt64();
          break;

        case DataType.Enum:
          var enumDefinition = df.EnumDefinitionTable[StructIndex]; // ?
          Value = df.ValueMap[r.ReadUInt32()];
          break;

        default:
          throw new NotImplementedException();
      }
    }
  }
}