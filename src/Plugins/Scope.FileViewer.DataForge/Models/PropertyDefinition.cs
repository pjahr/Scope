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
    public object Value { get; private set; } // -> Property
    public object Unknown { get; private set; } // -> Property

    public Property Read(BinaryReader r, DataForgeFile df)
    {
      object value;
      switch (DataType)
      {
        case DataType.Reference:
          Unknown = r.ReadUInt32(); // ?
          value = r.ReadGuid();
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.Locale:
          var offset = r.ReadUInt32();
          value = df.ValueMap[offset];
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.StrongPointer:
          r.ReadUInt32(); // ? {1:X8}
          value = r.ReadUInt32();
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.WeakPointer:
          var structIndex = r.ReadUInt32();
          var itemIndex = r.ReadUInt32();
          df.WeakMappings2.Add(new ClassMapping
          {
            StructIndex = (ushort)structIndex,
            RecordIndex = (int)itemIndex
          });
          return new Property { Name = Name, Type = DataType, Value = "TODO" };

        case DataType.String:
          var v = r.ReadUInt32();
          value = df.ValueMap[v];
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.Boolean:
          value = r.ReadByte();
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.Single:
          value = r.ReadSingle();
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.Double:
          value = r.ReadDouble();
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.Guid:
          value = r.ReadGuid();
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.SByte:
          value = r.ReadSByte();
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.Int16:
          value = r.ReadInt16();
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.Int32:
          value = r.ReadInt32();
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.Int64:
          value = r.ReadInt64();
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.Byte:
          value = r.ReadByte();
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.UInt16:
          value = r.ReadUInt16();
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.UInt32:
          value = r.ReadUInt32();
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.UInt64:
          value = r.ReadUInt64();
          return new Property { Name = Name, Type = DataType, Value = value };

        case DataType.Enum:
          var enumDefinition = df.EnumDefinitionTable[StructIndex]; // ?
          value = df.ValueMap[r.ReadUInt32()];
          return new Property { Name = Name, Type = DataType, Value = value };

        default:
          throw new NotImplementedException();
      }
    }

    public override string ToString()
    {
      return $"Prop: {Name} ({ConversionType}, {DataType})";
    }
  }
}
