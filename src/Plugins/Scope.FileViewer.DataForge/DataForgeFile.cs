using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Scope.FileViewer.DataForge
{
  internal class DataForgeFile
  {
    internal bool IsLegacy { get; set; }
    internal int FileVersion { get; set; }

    internal DataForgeStructDefinition[] StructDefinitionTable { get; set; }
    internal DataForgePropertyDefinition[] PropertyDefinitionTable { get; set; }
    internal DataForgeEnumDefinition[] EnumDefinitionTable { get; set; }
    internal DataForgeDataMapping[] DataMappingTable { get; set; }
    internal DataForgeRecord[] RecordDefinitionTable { get; set; }
    internal uint[] EnumOptionTable { get; set; }
    internal string[] ValueTable { get; set; }

    internal DataForgeReference[] Array_ReferenceValues { get; set; }
    internal Guid[] Array_GuidValues { get; set; }
    internal uint[] Array_StringValues { get; set; }
    internal uint[] Array_LocaleValues { get; set; }
    internal uint[] Array_EnumValues { get; set; }
    internal sbyte[] Array_Int8Values { get; set; }
    internal short[] Array_Int16Values { get; set; }
    internal int[] Array_Int32Values { get; set; }
    internal long[] Array_Int64Values { get; set; }
    internal byte[] Array_UInt8Values { get; set; }
    internal ushort[] Array_UInt16Values { get; set; }
    internal uint[] Array_UInt32Values { get; set; }
    internal ulong[] Array_UInt64Values { get; set; }
    internal bool[] Array_BooleanValues { get; set; }
    internal float[] Array_SingleValues { get; set; }
    internal double[] Array_DoubleValues { get; set; }
    internal DataForgePointer[] Array_StrongValues { get; set; }
    internal DataForgePointer[] Array_WeakValues { get; set; }

    internal Dictionary<uint, string> ValueMap { get; set; }

    internal List<ClassMapping> ClassMappings { get; set; }
    internal List<ClassMapping> StrongMappings { get; set; }
    internal List<ClassMapping> WeakMappings1 { get; set; }
    internal List<ClassMapping> WeakMappings2 { get; set; }

    public DataForgeFile(BinaryReader r, Boolean legacy = false)
    {
      IsLegacy = legacy;

      var temp00 = r.ReadInt32(); // ?

      FileVersion = r.ReadInt32();

      ClassMappings = new List<ClassMapping>();
      StrongMappings = new List<ClassMapping>();
      WeakMappings1 = new List<ClassMapping>();
      WeakMappings2 = new List<ClassMapping>();

      if (!IsLegacy)
      {
        var atemp1 = r.ReadUInt16();
        var atemp2 = r.ReadUInt16();
        var atemp3 = r.ReadUInt16();
        var atemp4 = r.ReadUInt16();
      }

      // item counts
      var structDefinitionCount = r.ReadInt32();
      var propertyDefinitionCount = r.ReadInt32();
      var enumDefinitionCount = r.ReadInt32();
      var dataMappingCount = r.ReadInt32();
      var recordDefinitionCount = r.ReadInt32();

      var booleanValueCount = r.ReadInt32();
      var int8ValueCount = r.ReadInt32();
      var int16ValueCount = r.ReadInt32();
      var int32ValueCount = r.ReadInt32();
      var int64ValueCount = r.ReadInt32();
      var uint8ValueCount = r.ReadInt32();
      var uint16ValueCount = r.ReadInt32();
      var uint32ValueCount = r.ReadInt32();
      var uint64ValueCount = r.ReadInt32();

      var singleValueCount = r.ReadInt32();
      var doubleValueCount = r.ReadInt32();
      var guidValueCount = r.ReadInt32();
      var stringValueCount = r.ReadInt32();
      var localeValueCount = r.ReadInt32();
      var enumValueCount = r.ReadInt32();
      var strongValueCount = r.ReadInt32();
      var weakValueCount = r.ReadInt32();

      var referenceValueCount = r.ReadInt32();
      var enumOptionCount = r.ReadInt32();
      var textLength = r.ReadUInt32();
      var unknown = (IsLegacy) ? 0 : r.ReadUInt32();

      StructDefinitionTable = structDefinitionCount.ToArray(() => new DataForgeStructDefinition(r));
      PropertyDefinitionTable = propertyDefinitionCount.ToArray(() => new DataForgePropertyDefinition(r));
      EnumDefinitionTable = enumDefinitionCount.ToArray(() => new DataForgeEnumDefinition(r));
      DataMappingTable = dataMappingCount.ToArray(() => new DataForgeDataMapping(r));
      RecordDefinitionTable = recordDefinitionCount.ToArray(() => new DataForgeRecord(r));

      Array_Int8Values = int8ValueCount.ToArray(() => r.ReadSByte());
      Array_Int16Values = int16ValueCount.ToArray(() => r.ReadInt16());
      Array_Int32Values = int32ValueCount.ToArray(() => r.ReadInt32());
      Array_Int64Values = int64ValueCount.ToArray(() => r.ReadInt64());
      Array_UInt8Values = uint8ValueCount.ToArray(() => r.ReadByte());
      Array_UInt16Values = uint16ValueCount.ToArray(() => r.ReadUInt16());
      Array_UInt32Values = uint32ValueCount.ToArray(() => r.ReadUInt32());
      Array_UInt64Values = uint64ValueCount.ToArray(() => r.ReadUInt64());
      Array_BooleanValues = booleanValueCount.ToArray(() => r.ReadBoolean());
      Array_SingleValues = singleValueCount.ToArray(() => r.ReadSingle());
      Array_DoubleValues = doubleValueCount.ToArray(() => r.ReadDouble());

      Array_GuidValues = guidValueCount.ToArray(() => r.ReadGuid());
      Array_StringValues = stringValueCount.ToArray(() => r.ReadUInt32());
      Array_LocaleValues = localeValueCount.ToArray(() => r.ReadUInt32());
      Array_EnumValues = enumValueCount.ToArray(() => r.ReadUInt32());
      Array_StrongValues = strongValueCount.ToArray(() => new DataForgePointer(r));
      Array_WeakValues = weakValueCount.ToArray(() => new DataForgePointer(r));

      Array_ReferenceValues = referenceValueCount.ToArray(() => new DataForgeReference(r));
      EnumOptionTable = enumOptionCount.ToArray(() => r.ReadUInt32());

      var values = new List<string>();

      var maxPosition = r.BaseStream.Position + textLength;
      var startPosition = r.BaseStream.Position;

      ValueMap = new Dictionary<uint, string> { };

      while (r.BaseStream.Position < maxPosition)
      {
        var offset = r.BaseStream.Position - startPosition;
        var value = r.ReadNullTerminatedString();
        values.Add(value);
        ValueMap[(uint)offset] = value;
      }

      ValueTable = values.ToArray();
    }
  }
}
