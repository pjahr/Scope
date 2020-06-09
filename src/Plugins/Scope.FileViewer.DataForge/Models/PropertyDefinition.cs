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

      return;
      string attribute="";
      switch (DataType)
      {
        case DataType.Reference:
          attribute = String.Format("{2}", DataType, r.ReadUInt32(), r.ReadGuid());
          break;
        case DataType.Locale:
          //attribute = String.Format("{1}", DataType, DocumentRoot.ValueMap[r.ReadUInt32()]);
          break;
        case DataType.StrongPointer:
          attribute = String.Format("{0}:{1:X8} {2:X8}", DataType, r.ReadUInt32(), r.ReadUInt32());
          break;
        case DataType.WeakPointer:
          var structIndex = r.ReadUInt32();
          var itemIndex = r.ReadUInt32();
          attribute = String.Format("{0}:{1:X8} {1:X8}", DataType, structIndex, itemIndex);
          //DocumentRoot.Require_WeakMapping2.Add(new ClassMapping {
          //  Node = attribute, StructIndex = (UInt16)structIndex, RecordIndex = (Int32)itemIndex
          //});
          break;
        case DataType.String:
          //attribute = String.Format("{1}", DataType, DocumentRoot.ValueMap[r.ReadUInt32()]);
          break;
        case DataType.Boolean:
          attribute = String.Format("{1}", DataType, r.ReadByte());
          break;
        case DataType.Single:
          attribute = String.Format("{1}", DataType, r.ReadSingle());
          break;
        case DataType.Double:
          attribute = String.Format("{1}", DataType, r.ReadDouble());
          break;
        case DataType.Guid:
          attribute = String.Format("{1}", DataType, r.ReadGuid());
          break;
        case DataType.SByte:
          attribute = String.Format("{1}", DataType, r.ReadSByte());
          break;
        case DataType.Int16:
          attribute = String.Format("{1}", DataType, r.ReadInt16());
          break;
        case DataType.Int32:
          attribute = String.Format("{1}", DataType, r.ReadInt32());
          break;
        case DataType.Int64:
          attribute = String.Format("{1}", DataType, r.ReadInt64());
          break;
        case DataType.Byte:
          attribute = String.Format("{1}", DataType, r.ReadByte());
          break;
        case DataType.UInt16:
          attribute = String.Format("{1}", DataType, r.ReadUInt16());
          break;
        case DataType.UInt32:
          attribute = String.Format("{1}", DataType, r.ReadUInt32());
          break;
        case DataType.UInt64:
          attribute = String.Format("{1}", DataType, r.ReadUInt64());
          break;
        case DataType.Enum:
          //var enumDefinition = DocumentRoot.EnumDefinitionTable[StructIndex];
          //attribute = String.Format("{1}", enumDefinition.Name, DocumentRoot.ValueMap[r.ReadUInt32()]);
          break;
        default:
          throw new NotImplementedException();


      }


      }

    public uint NameOffset { get; set; }
    public ushort StructIndex { get; set; }
    public DataType DataType { get; set; }
    public ConversionType ConversionType { get; set; }
    public ushort Padding { get; set; }

    public string Name => _valueOf(NameOffset);
  }
}